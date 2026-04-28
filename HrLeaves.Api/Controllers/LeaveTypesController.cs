using HrLeaves.Api.Data;
using HrLeaves.Api.DTOs;
using HrLeaves.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrLeaves.Api.Controllers;

[ApiController]
[Route("api/leave-types")]
public class LeaveTypesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await db.LeaveTypes.OrderBy(x => x.Name).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(LeaveTypeUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Name is required." });
        var entity = new LeaveType { Name = request.Name.Trim(), DefaultDays = request.DefaultDays, IsAccrued = request.IsAccrued, MonthlyAccrualRate = request.MonthlyAccrualRate, IsActive = request.IsActive };
        db.LeaveTypes.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LeaveTypeUpsertRequest request)
    {
        var entity = await db.LeaveTypes.FindAsync(id);
        if (entity is null) return NotFound(new { message = "Leave type not found." });
        entity.Name = request.Name.Trim();
        entity.DefaultDays = request.DefaultDays;
        entity.IsAccrued = request.IsAccrued;
        entity.MonthlyAccrualRate = request.MonthlyAccrualRate;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync();
        return Ok(entity);
    }
}
