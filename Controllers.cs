// Controllers/AuthController.cs
using LeaveManagement.DTOs;
using LeaveManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var (success, message, data) = await _authService.LoginAsync(dto);
            if (!success) return Unauthorized(new { message });
            return Ok(new { message, data });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            var (success, message) = await _authService.RegisterAsync(dto);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }
    }
}


// Controllers/LeaveController.cs
using System.Security.Claims;
using LeaveManagement.DTOs;
using LeaveManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        private int GetUserID() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // POST api/leave/apply
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] CreateLeaveRequestDTO dto)
        {
            var (success, message, data) = await _leaveService.ApplyLeaveAsync(GetUserID(), dto);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, data });
        }

        // GET api/leave/my-requests
        [HttpGet("my-requests")]
        public async Task<IActionResult> MyRequests()
        {
            var data = await _leaveService.GetMyLeavesAsync(GetUserID());
            return Ok(data);
        }

        // GET api/leave/my-balance?year=2025
        [HttpGet("my-balance")]
        public async Task<IActionResult> MyBalance([FromQuery] int year = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            var data = await _leaveService.GetLeaveBalancesAsync(GetUserID(), year);
            return Ok(data);
        }

        // DELETE api/leave/cancel/5
        [HttpDelete("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var (success, message) = await _leaveService.CancelLeaveAsync(id, GetUserID());
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        // GET api/leave/pending-reviews  (Manager)
        [HttpGet("pending-reviews")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> PendingReviews()
        {
            var data = await _leaveService.GetPendingRequestsAsync(GetUserID());
            return Ok(data);
        }

        // PUT api/leave/review/5  (Manager)
        [HttpPut("review/{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Review(int id, [FromBody] ReviewLeaveRequestDTO dto)
        {
            var (success, message) = await _leaveService.ReviewLeaveAsync(id, GetUserID(), dto);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        // GET api/leave/all  (Admin)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllRequests()
        {
            var data = await _leaveService.GetAllRequestsAsync();
            return Ok(data);
        }
    }
}


// Controllers/NotificationController.cs
using System.Security.Claims;
using LeaveManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserID() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifs = await _context.Notifications
                .Where(n => n.UserID == GetUserID())
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.NotificationID,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    n.CreatedAt
                }).ToListAsync();
            return Ok(notifs);
        }

        [HttpPut("mark-read/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var notif = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationID == id && n.UserID == GetUserID());
            if (notif == null) return NotFound();
            notif.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Marked as read" });
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var notifs = await _context.Notifications
                .Where(n => n.UserID == GetUserID() && !n.IsRead)
                .ToListAsync();
            notifs.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync();
            return Ok(new { message = "All notifications marked as read" });
        }
    }
}
