using HrLeaves.Api.DTOs;
using HrLeaves.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HrLeaves.Api.Controllers;

[ApiController]
[Route("api/leave-requests")]
public class LeaveRequestsController(ILeaveService service) : ControllerBase
{
    /// <summary>Search leave requests with filters for dashboard and approval screens.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<LeaveRequestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] int? employeeId, [FromQuery] string? status, [FromQuery] int? leaveTypeId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await service.SearchAsync(employeeId, status, leaveTypeId, from, to));

    /// <summary>Submit a leave request. Days are calculated automatically excluding weekends.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<LeaveRequestDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Apply(ApplyLeaveRequest request)
    {
        var result = await service.ApplyAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await service.ApproveAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, LeaveDecisionRequest request)
    {
        var result = await service.RejectAsync(id, request.Comment);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("bulk-approve")]
    public async Task<IActionResult> BulkApprove(BulkDecisionRequest request)
    {
        var result = await service.BulkApproveAsync(request.RequestIds);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("bulk-reject")]
    public async Task<IActionResult> BulkReject(BulkDecisionRequest request)
    {
        var result = await service.BulkRejectAsync(request.RequestIds, request.Comment);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
