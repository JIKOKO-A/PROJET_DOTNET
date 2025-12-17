using Microsoft.EntityFrameworkCore;
using HRManagementApp.Core;

namespace HRManagementApp.Data;

public class HRDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=HRDatabase.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            
            entity.HasOne(d => d.Manager)
                  .WithMany()
                  .HasForeignKey(d => d.ManagerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Attendance configuration
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(a => a.Id);
            
            entity.HasOne(a => a.Employee)
                  .WithMany(e => e.Attendances)
                  .HasForeignKey(a => a.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // LeaveRequest configuration
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(l => l.Id);
            
            entity.HasOne(l => l.Employee)
                  .WithMany(e => e.LeaveRequests)
                  .HasForeignKey(l => l.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Payroll configuration
        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.BaseSalary).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Deductions).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Bonuses).HasColumnType("decimal(18,2)");
            entity.Property(p => p.NetSalary).HasColumnType("decimal(18,2)");
            
            entity.HasOne(p => p.Employee)
                  .WithMany(e => e.Payrolls)
                  .HasForeignKey(p => p.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}


