// Models/User.cs
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Models
{
    public class User
    {
        public int UserID { get; set; }
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public int? DepartmentID { get; set; }
        public int? ManagerID { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Role Role { get; set; } = null!;
        public Department? Department { get; set; }
        public User? Manager { get; set; }
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    }

    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public ICollection<User> Users { get; set; } = new List<User>();
    }

    public class Department
    {
        public int DepartmentID { get; set; }
        [Required] public string DepartmentName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<User> Users { get; set; } = new List<User>();
    }

    public class LeaveType
    {
        public int LeaveTypeID { get; set; }
        [Required] public string TypeName { get; set; } = string.Empty;
        public int MaxDaysPerYear { get; set; }
        public bool IsPaid { get; set; } = true;
        public string? Description { get; set; }
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    }

    public class LeaveRequest
    {
        public int RequestID { get; set; }
        public int UserID { get; set; }
        public int LeaveTypeID { get; set; }
        [Required] public DateTime StartDate { get; set; }
        [Required] public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        [Required] public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public LeaveType LeaveType { get; set; } = null!;
        public User? Reviewer { get; set; }
    }

    public class LeaveBalance
    {
        public int BalanceID { get; set; }
        public int UserID { get; set; }
        public int LeaveTypeID { get; set; }
        public int Year { get; set; }
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public int RemainingDays => TotalDays - UsedDays;

        public User User { get; set; } = null!;
        public LeaveType LeaveType { get; set; } = null!;
    }

    public class Notification
    {
        public int NotificationID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
    }
}
