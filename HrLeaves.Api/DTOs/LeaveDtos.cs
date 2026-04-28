using HrLeaves.Api.Models;

namespace HrLeaves.Api.DTOs;

public record ApplyLeaveRequest(int EmployeeId, int LeaveTypeId, DateTime StartDate, DateTime EndDate, string Reason);
public record LeaveDecisionRequest(string? Comment);
public record BulkDecisionRequest(List<int> RequestIds, string? Comment);
public record LeaveTypeUpsertRequest(string Name, decimal DefaultDays, bool IsAccrued, decimal MonthlyAccrualRate, bool IsActive);
public record SettlementRequest(int EmployeeId, int LeaveTypeId, decimal AdjustmentDays, string Reason);
public record ApiResult<T>(bool Success, string Message, T? Data);

public record LeaveRequestDto(
    int Id,
    int EmployeeId,
    string EmployeeName,
    int LeaveTypeId,
    string LeaveTypeName,
    DateTime StartDate,
    DateTime EndDate,
    decimal DaysRequested,
    string Reason,
    LeaveRequestStatus Status,
    string? RejectionComment);

public record LeaveBalanceDto(int EmployeeId, int LeaveTypeId, string LeaveTypeName, decimal AllocatedDays, decimal UsedDays, decimal PendingDays, decimal AvailableDays);
