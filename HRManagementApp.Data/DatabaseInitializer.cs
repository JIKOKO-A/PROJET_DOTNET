using HRManagementApp.Core;

namespace HRManagementApp.Data;

public static class DatabaseInitializer
{
    public static void Initialize(HRDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Departments.Any())
        {
            return; // Database already seeded
        }

        // Seed Users (Admin and HR)
        var users = new[]
        {
            new User
            {
                Username = "admin",
                PasswordHash = AuthenticationService.HashPassword("admin123"),
                FullName = "System Administrator",
                Email = "admin@hrmanagement.com",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedDate = DateTime.Now
            },
            new User
            {
                Username = "hr",
                PasswordHash = AuthenticationService.HashPassword("hr123"),
                FullName = "HR Manager",
                Email = "hr@hrmanagement.com",
                Role = UserRole.HR,
                IsActive = true,
                CreatedDate = DateTime.Now
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        // Seed Departments
        var departments = new[]
        {
            new Department { Name = "IT", Description = "Information Technology" },
            new Department { Name = "HR", Description = "Human Resources" },
            new Department { Name = "Finance", Description = "Finance and Accounting" },
            new Department { Name = "Sales", Description = "Sales and Marketing" }
        };

        context.Departments.AddRange(departments);
        context.SaveChanges();

        // Seed Employees
        var employees = new[]
        {
            new Employee
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@company.com",
                Phone = "123-456-7890",
                HireDate = new DateTime(2020, 1, 15),
                DepartmentId = 1,
                Position = "Software Developer",
                Salary = 75000
            },
            new Employee
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@company.com",
                Phone = "123-456-7891",
                HireDate = new DateTime(2019, 3, 20),
                DepartmentId = 2,
                Position = "HR Manager",
                Salary = 85000
            },
            new Employee
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@company.com",
                Phone = "123-456-7892",
                HireDate = new DateTime(2021, 6, 10),
                DepartmentId = 3,
                Position = "Accountant",
                Salary = 65000
            }
        };

        context.Employees.AddRange(employees);
        context.SaveChanges();
    }
}


