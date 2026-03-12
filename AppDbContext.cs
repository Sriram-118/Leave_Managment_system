// Data/AppDbContext.cs
using LeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User self-referencing (Manager)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Manager)
                .WithMany()
                .HasForeignKey(u => u.ManagerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentID)
                .OnDelete(DeleteBehavior.SetNull);

            // LeaveRequest reviewer
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.Reviewer)
                .WithMany()
                .HasForeignKey(lr => lr.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.User)
                .WithMany(u => u.LeaveRequests)
                .HasForeignKey(lr => lr.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique balance per user per leave type per year
            modelBuilder.Entity<LeaveBalance>()
                .HasIndex(lb => new { lb.UserID, lb.LeaveTypeID, lb.Year })
                .IsUnique();

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleID = 1, RoleName = "Admin" },
                new Role { RoleID = 2, RoleName = "Manager" },
                new Role { RoleID = 3, RoleName = "Employee" }
            );

            // Seed LeaveTypes
            modelBuilder.Entity<LeaveType>().HasData(
                new LeaveType { LeaveTypeID = 1, TypeName = "Annual Leave",    MaxDaysPerYear = 21, IsPaid = true },
                new LeaveType { LeaveTypeID = 2, TypeName = "Sick Leave",      MaxDaysPerYear = 14, IsPaid = true },
                new LeaveType { LeaveTypeID = 3, TypeName = "Maternity Leave", MaxDaysPerYear = 90, IsPaid = true },
                new LeaveType { LeaveTypeID = 4, TypeName = "Paternity Leave", MaxDaysPerYear = 14, IsPaid = true },
                new LeaveType { LeaveTypeID = 5, TypeName = "Unpaid Leave",    MaxDaysPerYear = 30, IsPaid = false },
                new LeaveType { LeaveTypeID = 6, TypeName = "Emergency Leave", MaxDaysPerYear = 3,  IsPaid = true }
            );
        }
    }
}
