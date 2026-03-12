// Interfaces/ILeaveService.cs
using LeaveManagement.DTOs;

namespace LeaveManagement.Interfaces
{
    public interface ILeaveService
    {
        Task<(bool Success, string Message, LeaveRequestResponseDTO? Data)> ApplyLeaveAsync(int userID, CreateLeaveRequestDTO dto);
        Task<IEnumerable<LeaveRequestResponseDTO>> GetMyLeavesAsync(int userID);
        Task<IEnumerable<LeaveRequestResponseDTO>> GetPendingRequestsAsync(int managerID);
        Task<IEnumerable<LeaveRequestResponseDTO>> GetAllRequestsAsync();
        Task<(bool Success, string Message)> ReviewLeaveAsync(int requestID, int reviewerID, ReviewLeaveRequestDTO dto);
        Task<(bool Success, string Message)> CancelLeaveAsync(int requestID, int userID);
        Task<IEnumerable<LeaveBalanceDTO>> GetLeaveBalancesAsync(int userID, int year);
        Task InitialiseBalancesAsync(int userID, int year);
    }
}

// Services/LeaveService.cs
using LeaveManagement.Data;
using LeaveManagement.DTOs;
using LeaveManagement.Interfaces;
using LeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _context;

        public LeaveService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, LeaveRequestResponseDTO? Data)> ApplyLeaveAsync(int userID, CreateLeaveRequestDTO dto)
        {
            // Date validation
            if (dto.StartDate.Date < DateTime.Today)
                return (false, "Start date cannot be in the past", null);

            if (dto.EndDate.Date < dto.StartDate.Date)
                return (false, "End date must be after start date", null);

            int totalDays = (int)(dto.EndDate.Date - dto.StartDate.Date).TotalDays + 1;

            // Check balance
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.UserID == userID
                    && b.LeaveTypeID == dto.LeaveTypeID
                    && b.Year == dto.StartDate.Year);

            if (balance == null)
                return (false, "No leave balance found for this leave type", null);

            if (balance.RemainingDays < totalDays)
                return (false, $"Insufficient leave balance. You have {balance.RemainingDays} days remaining", null);

            // Check for overlapping requests
            bool hasOverlap = await _context.LeaveRequests
                .AnyAsync(lr => lr.UserID == userID
                    && lr.Status != "Cancelled"
                    && lr.Status != "Rejected"
                    && lr.StartDate < dto.EndDate
                    && lr.EndDate > dto.StartDate);

            if (hasOverlap)
                return (false, "You already have a leave request overlapping these dates", null);

            var request = new LeaveRequest
            {
                UserID      = userID,
                LeaveTypeID = dto.LeaveTypeID,
                StartDate   = dto.StartDate,
                EndDate     = dto.EndDate,
                TotalDays   = totalDays,
                Reason      = dto.Reason,
                Status      = "Pending"
            };

            _context.LeaveRequests.Add(request);
            await _context.SaveChangesAsync();

            var result = await MapToResponseDTO(request);
            return (true, "Leave request submitted successfully", result);
        }

        public async Task<IEnumerable<LeaveRequestResponseDTO>> GetMyLeavesAsync(int userID)
        {
            var requests = await _context.LeaveRequests
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.User)
                .Include(lr => lr.Reviewer)
                .Where(lr => lr.UserID == userID)
                .OrderByDescending(lr => lr.AppliedAt)
                .ToListAsync();

            return requests.Select(MapToDTO);
        }

        public async Task<IEnumerable<LeaveRequestResponseDTO>> GetPendingRequestsAsync(int managerID)
        {
            var requests = await _context.LeaveRequests
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.User)
                .Where(lr => lr.User.ManagerID == managerID && lr.Status == "Pending")
                .OrderBy(lr => lr.StartDate)
                .ToListAsync();

            return requests.Select(MapToDTO);
        }

        public async Task<IEnumerable<LeaveRequestResponseDTO>> GetAllRequestsAsync()
        {
            var requests = await _context.LeaveRequests
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.User)
                .Include(lr => lr.Reviewer)
                .OrderByDescending(lr => lr.AppliedAt)
                .ToListAsync();

            return requests.Select(MapToDTO);
        }

        public async Task<(bool Success, string Message)> ReviewLeaveAsync(int requestID, int reviewerID, ReviewLeaveRequestDTO dto)
        {
            var request = await _context.LeaveRequests.FindAsync(requestID);
            if (request == null) return (false, "Leave request not found");
            if (request.Status != "Pending") return (false, "Only pending requests can be reviewed");

            if (dto.Status != "Approved" && dto.Status != "Rejected")
                return (false, "Status must be Approved or Rejected");

            request.Status     = dto.Status;
            request.ReviewedBy = reviewerID;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewNote = dto.ReviewNote;
            request.UpdatedAt  = DateTime.UtcNow;

            if (dto.Status == "Approved")
            {
                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b => b.UserID == request.UserID
                        && b.LeaveTypeID == request.LeaveTypeID
                        && b.Year == request.StartDate.Year);

                if (balance != null)
                    balance.UsedDays += request.TotalDays;
            }

            _context.Notifications.Add(new Notification
            {
                UserID  = request.UserID,
                Title   = $"Leave Request {dto.Status}",
                Message = $"Your leave from {request.StartDate:dd MMM yyyy} to {request.EndDate:dd MMM yyyy} has been {dto.Status.ToLower()}"
            });

            await _context.SaveChangesAsync();
            return (true, $"Leave request {dto.Status.ToLower()} successfully");
        }

        public async Task<(bool Success, string Message)> CancelLeaveAsync(int requestID, int userID)
        {
            var request = await _context.LeaveRequests.FindAsync(requestID);
            if (request == null) return (false, "Leave request not found");
            if (request.UserID != userID) return (false, "Unauthorized");
            if (request.Status == "Cancelled") return (false, "Request is already cancelled");
            if (request.StartDate.Date <= DateTime.Today) return (false, "Cannot cancel a leave that has already started");

            if (request.Status == "Approved")
            {
                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b => b.UserID == userID
                        && b.LeaveTypeID == request.LeaveTypeID
                        && b.Year == request.StartDate.Year);
                if (balance != null)
                    balance.UsedDays -= request.TotalDays;
            }

            request.Status    = "Cancelled";
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Leave request cancelled successfully");
        }

        public async Task<IEnumerable<LeaveBalanceDTO>> GetLeaveBalancesAsync(int userID, int year)
        {
            var balances = await _context.LeaveBalances
                .Include(b => b.LeaveType)
                .Where(b => b.UserID == userID && b.Year == year)
                .ToListAsync();

            return balances.Select(b => new LeaveBalanceDTO
            {
                LeaveType     = b.LeaveType.TypeName,
                IsPaid        = b.LeaveType.IsPaid,
                TotalDays     = b.TotalDays,
                UsedDays      = b.UsedDays,
                RemainingDays = b.RemainingDays,
                Year          = b.Year
            });
        }

        public async Task InitialiseBalancesAsync(int userID, int year)
        {
            var leaveTypes = await _context.LeaveTypes.ToListAsync();
            foreach (var lt in leaveTypes)
            {
                bool exists = await _context.LeaveBalances
                    .AnyAsync(b => b.UserID == userID && b.LeaveTypeID == lt.LeaveTypeID && b.Year == year);
                if (!exists)
                {
                    _context.LeaveBalances.Add(new LeaveBalance
                    {
                        UserID      = userID,
                        LeaveTypeID = lt.LeaveTypeID,
                        Year        = year,
                        TotalDays   = lt.MaxDaysPerYear,
                        UsedDays    = 0
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        private LeaveRequestResponseDTO MapToDTO(LeaveRequest lr) => new()
        {
            RequestID    = lr.RequestID,
            EmployeeName = $"{lr.User.FirstName} {lr.User.LastName}",
            LeaveType    = lr.LeaveType.TypeName,
            StartDate    = lr.StartDate,
            EndDate      = lr.EndDate,
            TotalDays    = lr.TotalDays,
            Reason       = lr.Reason,
            Status       = lr.Status,
            ReviewNote   = lr.ReviewNote,
            ReviewedBy   = lr.Reviewer != null ? $"{lr.Reviewer.FirstName} {lr.Reviewer.LastName}" : null,
            AppliedAt    = lr.AppliedAt
        };

        private async Task<LeaveRequestResponseDTO> MapToResponseDTO(LeaveRequest lr)
        {
            await _context.Entry(lr).Reference(x => x.User).LoadAsync();
            await _context.Entry(lr).Reference(x => x.LeaveType).LoadAsync();
            return MapToDTO(lr);
        }
    }
}
