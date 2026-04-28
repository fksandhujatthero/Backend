using HrLeaves.Api.Models;

namespace HrLeaves.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Employees.Any())
        {
            db.Employees.Add(new Employee { FullName = "Demo Employee", Email = "employee@example.com", HireDate = DateTime.UtcNow.AddMonths(-18) });
            db.SaveChanges();
        }
        if (!db.LeaveTypes.Any())
        {
            db.LeaveTypes.AddRange(
                new LeaveType { Name = "Vacation", DefaultDays = 15, IsAccrued = true, MonthlyAccrualRate = 1.25m },
                new LeaveType { Name = "Sick", DefaultDays = 10, IsAccrued = false },
                new LeaveType { Name = "Maternity", DefaultDays = 90, IsAccrued = false });
            db.SaveChanges();
        }
        var employee = db.Employees.First();
        foreach (var leaveType in db.LeaveTypes.ToList())
        {
            if (!db.LeaveBalances.Any(b => b.EmployeeId == employee.Id && b.LeaveTypeId == leaveType.Id))
            {
                var allocated = leaveType.IsAccrued
                    ? Math.Min(leaveType.DefaultDays, MonthsBetween(employee.HireDate, DateTime.UtcNow) * leaveType.MonthlyAccrualRate)
                    : leaveType.DefaultDays;
                db.LeaveBalances.Add(new LeaveBalance { EmployeeId = employee.Id, LeaveTypeId = leaveType.Id, AllocatedDays = allocated });
            }
        }
        db.SaveChanges();
    }

    private static int MonthsBetween(DateTime from, DateTime to) => Math.Max(0, ((to.Year - from.Year) * 12) + to.Month - from.Month);
}
