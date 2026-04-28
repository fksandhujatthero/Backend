namespace HrLeaves.Api.Models;

public class LeaveType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DefaultDays { get; set; }
    public bool IsAccrued { get; set; }
    public decimal MonthlyAccrualRate { get; set; }
    public bool IsActive { get; set; } = true;
}
