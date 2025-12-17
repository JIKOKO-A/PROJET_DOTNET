namespace HRManagementApp.Core;

public class Payroll
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal Deductions { get; set; }
    public decimal Bonuses { get; set; }
    public decimal NetSalary { get; set; }
    
    // Navigation property
    public Employee Employee { get; set; } = null!;
}


