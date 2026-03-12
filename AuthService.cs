// Services/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LeaveManagement.Data;
using LeaveManagement.DTOs;
using LeaveManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LeaveManagement.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config  = config;
        }

        public async Task<(bool Success, string Message, AuthResponseDTO? Data)> LoginAsync(LoginDTO dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return (false, "Invalid email or password", null);

            var token = GenerateJwtToken(user);

            return (true, "Login successful", new AuthResponseDTO
            {
                Token    = token,
                Email    = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Role     = user.Role.RoleName,
                UserID   = user.UserID
            });
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDTO dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return (false, "Email already registered");

            var user = new User
            {
                FirstName    = dto.FirstName,
                LastName     = dto.LastName,
                Email        = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleID       = dto.RoleID,
                DepartmentID = dto.DepartmentID,
                ManagerID    = dto.ManagerID
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Initialise leave balances for current year
            var leaveTypes = await _context.LeaveTypes.ToListAsync();
            foreach (var lt in leaveTypes)
            {
                _context.LeaveBalances.Add(new LeaveBalance
                {
                    UserID      = user.UserID,
                    LeaveTypeID = lt.LeaveTypeID,
                    Year        = DateTime.Now.Year,
                    TotalDays   = lt.MaxDaysPerYear,
                    UsedDays    = 0
                });
            }
            await _context.SaveChangesAsync();

            return (true, "Registration successful");
        }

        private string GenerateJwtToken(User user)
        {
            var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(8);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role.RoleName),
                new Claim("FullName",                $"{user.FirstName} {user.LastName}")
            };

            var token = new JwtSecurityToken(
                issuer:   _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims:   claims,
                expires:  expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
