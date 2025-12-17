namespace HRManagementApp.Core;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime HireDate { get; set; }
    public int? DepartmentId { get; set; }
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    
    // Navigation properties
    public Department? Department { get; set; }
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    
    public string FullName => $"{FirstName} {LastName}";
}


