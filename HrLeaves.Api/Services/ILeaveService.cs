using HrLeaves.Api.DTOs;
using HrLeaves.Api.Models;

namespace HrLeaves.Api.Services;

public interface ILeaveService
{
    Task<ApiResult<LeaveRequestDto>> ApplyAsync(ApplyLeaveRequest request);
    Task<ApiResult<LeaveRequestDto>> ApproveAsync(int requestId);
    Task<ApiResult<LeaveRequestDto>> RejectAsync(int requestId, string? comment);
    Task<ApiResult<List<LeaveRequestDto>>> BulkApproveAsync(List<int> ids);
    Task<ApiResult<List<LeaveRequestDto>>> BulkRejectAsync(List<int> ids, string? comment);
    Task<List<LeaveRequestDto>> SearchAsync(int? employeeId, string? status, int? leaveTypeId, DateTime? from, DateTime? to);
    Task<List<LeaveBalanceDto>> GetBalancesAsync(int employeeId);
    Task<ApiResult<LeaveSettlement>> SettleAsync(SettlementRequest request);
}
