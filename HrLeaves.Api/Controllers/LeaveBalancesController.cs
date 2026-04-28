using HrLeaves.Api.DTOs;
using HrLeaves.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HrLeaves.Api.Controllers;

[ApiController]
[Route("api/leave-balances")]
public class LeaveBalancesController(ILeaveService service) : ControllerBase
{
    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployee(int employeeId) => Ok(await service.GetBalancesAsync(employeeId));

    [HttpPost("settlements")]
    public async Task<IActionResult> Settle(SettlementRequest request)
    {
        var result = await service.SettleAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
