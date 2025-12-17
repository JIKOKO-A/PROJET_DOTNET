namespace HRManagementApp.Core;

public enum LeaveType
{
    Vacation,
    Sick,
    Personal,
    Maternity,
    Paternity,
    Unpaid
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected
}

public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LeaveType LeaveType { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? Reason { get; set; }
    
    // Navigation property
    public Employee Employee { get; set; } = null!;
    
    public int DaysRequested => (EndDate - StartDate).Days + 1;
}


