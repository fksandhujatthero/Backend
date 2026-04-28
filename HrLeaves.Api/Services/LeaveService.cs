using HrLeaves.Api.Data;
using HrLeaves.Api.DTOs;
using HrLeaves.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HrLeaves.Api.Services;

public class LeaveService(AppDbContext db) : ILeaveService
{
    public async Task<ApiResult<LeaveRequestDto>> ApplyAsync(ApplyLeaveRequest request)
    {
        if (request.StartDate.Date > request.EndDate.Date) return Fail("StartDate must be less than or equal to EndDate.");
        var days = BusinessDays(request.StartDate, request.EndDate);
        if (days <= 0) return Fail("Leave request must contain at least one business day.");

        var employeeExists = await db.Employees.AnyAsync(e => e.Id == request.EmployeeId);
        if (!employeeExists) return Fail("Employee not found.");

        var balance = await db.LeaveBalances.FirstOrDefaultAsync(b => b.EmployeeId == request.EmployeeId && b.LeaveTypeId == request.LeaveTypeId);
        if (balance is null) return Fail("Leave balance not found for the selected leave type.");
        if (balance.AvailableDays < days) return Fail("Insufficient leave balance.");

        var overlaps = await db.LeaveRequests.AnyAsync(r => r.EmployeeId == request.EmployeeId
            && r.Status != LeaveRequestStatus.Rejected && r.Status != LeaveRequestStatus.Cancelled
            && request.StartDate.Date <= r.EndDate.Date && request.EndDate.Date >= r.StartDate.Date);
        if (overlaps) return Fail("Leave dates overlap with an existing leave request.");

        var entity = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            DaysRequested = days,
            Reason = request.Reason.Trim(),
            Status = LeaveRequestStatus.Pending
        };
        balance.PendingDays += days;
        db.LeaveRequests.Add(entity);
        await db.SaveChangesAsync();
        return new(true, "Leave request submitted successfully.", await ToDto(entity.Id));
    }

    public Task<ApiResult<LeaveRequestDto>> ApproveAsync(int requestId) => DecideAsync(requestId, true, null);
    public Task<ApiResult<LeaveRequestDto>> RejectAsync(int requestId, string? comment) => DecideAsync(requestId, false, comment);

    public async Task<ApiResult<List<LeaveRequestDto>>> BulkApproveAsync(List<int> ids) => await BulkDecideAsync(ids, true, null);
    public async Task<ApiResult<List<LeaveRequestDto>>> BulkRejectAsync(List<int> ids, string? comment) => await BulkDecideAsync(ids, false, comment);

    public async Task<List<LeaveRequestDto>> SearchAsync(int? employeeId, string? status, int? leaveTypeId, DateTime? from, DateTime? to)
    {
        var query = db.LeaveRequests.Include(r => r.Employee).Include(r => r.LeaveType).AsQueryable();
        if (employeeId.HasValue) query = query.Where(r => r.EmployeeId == employeeId);
        if (leaveTypeId.HasValue) query = query.Where(r => r.LeaveTypeId == leaveTypeId);
        if (from.HasValue) query = query.Where(r => r.StartDate >= from.Value.Date);
        if (to.HasValue) query = query.Where(r => r.EndDate <= to.Value.Date);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LeaveRequestStatus>(status, true, out var parsed)) query = query.Where(r => r.Status == parsed);
        return await query.OrderByDescending(r => r.CreatedAt).Select(r => Map(r)).ToListAsync();
    }

    public async Task<List<LeaveBalanceDto>> GetBalancesAsync(int employeeId)
    {
        return await db.LeaveBalances.Include(b => b.LeaveType).Where(b => b.EmployeeId == employeeId)
            .Select(b => new LeaveBalanceDto(b.EmployeeId, b.LeaveTypeId, b.LeaveType.Name, b.AllocatedDays, b.UsedDays, b.PendingDays, b.AvailableDays)).ToListAsync();
    }

    public async Task<ApiResult<LeaveSettlement>> SettleAsync(SettlementRequest request)
    {
        var balance = await db.LeaveBalances.FirstOrDefaultAsync(b => b.EmployeeId == request.EmployeeId && b.LeaveTypeId == request.LeaveTypeId);
        if (balance is null) return new(false, "Leave balance not found.", null);
        balance.AllocatedDays += request.AdjustmentDays;
        var settlement = new LeaveSettlement { EmployeeId = request.EmployeeId, LeaveTypeId = request.LeaveTypeId, AdjustmentDays = request.AdjustmentDays, Reason = request.Reason };
        db.LeaveSettlements.Add(settlement);
        await db.SaveChangesAsync();
        return new(true, "Leave balance adjusted successfully.", settlement);
    }

    private async Task<ApiResult<List<LeaveRequestDto>>> BulkDecideAsync(List<int> ids, bool approve, string? comment)
    {
        var output = new List<LeaveRequestDto>();
        foreach (var id in ids.Distinct())
        {
            var result = await DecideAsync(id, approve, comment);
            if (!result.Success) return new(false, result.Message, output);
            output.Add(result.Data!);
        }
        return new(true, approve ? "Selected requests approved." : "Selected requests rejected.", output);
    }

    private async Task<ApiResult<LeaveRequestDto>> DecideAsync(int requestId, bool approve, string? comment)
    {
        var request = await db.LeaveRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request is null) return Fail("Leave request not found.");
        if (request.Status != LeaveRequestStatus.Pending) return Fail("Only pending requests can be decided.");
        var balance = await db.LeaveBalances.FirstAsync(b => b.EmployeeId == request.EmployeeId && b.LeaveTypeId == request.LeaveTypeId);
        balance.PendingDays -= request.DaysRequested;
        if (approve)
        {
            var conflict = await db.LeaveRequests.AnyAsync(r => r.Id != request.Id && r.EmployeeId == request.EmployeeId && r.Status == LeaveRequestStatus.Approved && request.StartDate <= r.EndDate && request.EndDate >= r.StartDate);
            if (conflict) return Fail("Conflict detected with an approved leave request.");
            request.Status = LeaveRequestStatus.Approved;
            balance.UsedDays += request.DaysRequested;
        }
        else
        {
            request.Status = LeaveRequestStatus.Rejected;
            request.RejectionComment = comment;
        }
        request.DecidedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return new(true, approve ? "Leave request approved." : "Leave request rejected.", await ToDto(request.Id));
    }

    private static decimal BusinessDays(DateTime start, DateTime end)
    {
        var count = 0;
        for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday) count++;
        return count;
    }

    private async Task<LeaveRequestDto?> ToDto(int id) => await db.LeaveRequests.Include(r => r.Employee).Include(r => r.LeaveType).Where(r => r.Id == id).Select(r => Map(r)).FirstOrDefaultAsync();
    private static LeaveRequestDto Map(LeaveRequest r) => new(r.Id, r.EmployeeId, r.Employee.FullName, r.LeaveTypeId, r.LeaveType.Name, r.StartDate, r.EndDate, r.DaysRequested, r.Reason, r.Status, r.RejectionComment);
    private static ApiResult<LeaveRequestDto> Fail(string message) => new(false, message, null);
}
