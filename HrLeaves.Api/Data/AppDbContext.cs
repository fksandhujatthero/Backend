using HrLeaves.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HrLeaves.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveSettlement> LeaveSettlements => Set<LeaveSettlement>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Employee>().HasIndex(e => e.Email).IsUnique();
        builder.Entity<LeaveType>().Property(x => x.DefaultDays).HasPrecision(8, 2);
        builder.Entity<LeaveType>().Property(x => x.MonthlyAccrualRate).HasPrecision(8, 2);
        builder.Entity<LeaveBalance>().Property(x => x.AllocatedDays).HasPrecision(8, 2);
        builder.Entity<LeaveBalance>().Property(x => x.UsedDays).HasPrecision(8, 2);
        builder.Entity<LeaveBalance>().Property(x => x.PendingDays).HasPrecision(8, 2);
        builder.Entity<LeaveBalance>().HasIndex(x => new { x.EmployeeId, x.LeaveTypeId }).IsUnique();
        builder.Entity<LeaveRequest>().Property(x => x.DaysRequested).HasPrecision(8, 2);
        builder.Entity<LeaveSettlement>().Property(x => x.AdjustmentDays).HasPrecision(8, 2);
    }
}
