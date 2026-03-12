// DTOs/AuthDTOs.cs
namespace LeaveManagement.DTOs
{
    public class LoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleID { get; set; } = 3;
        public int? DepartmentID { get; set; }
        public int? ManagerID { get; set; }
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserID { get; set; }
    }

    // Leave Request DTOs
    public class CreateLeaveRequestDTO
    {
        public int LeaveTypeID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class ReviewLeaveRequestDTO
    {
        public string Status { get; set; } = string.Empty;  // Approved or Rejected
        public string? ReviewNote { get; set; }
    }

    public class LeaveRequestResponseDTO
    {
        public int RequestID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ReviewNote { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    // Leave Balance DTO
    public class LeaveBalanceDTO
    {
        public string LeaveType { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public int RemainingDays { get; set; }
        public int Year { get; set; }
    }

    // Notification DTO
    public class NotificationDTO
    {
        public int NotificationID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // User profile DTO
    public class UserProfileDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? ManagerName { get; set; }
    }
}
