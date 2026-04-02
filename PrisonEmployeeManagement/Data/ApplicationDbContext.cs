using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeConduct> EmployeeConducts { get; set; }
        public DbSet<EmployeePosting> EmployeePostings { get; set; }
        public DbSet<EmployeeTraining> EmployeeTrainings { get; set; }
        public DbSet<EmployeeAward> EmployeeAwards { get; set; }
        public DbSet<EmployeeLeave> EmployeeLeaves { get; set; }
        public DbSet<EFile> EFiles { get; set; }
        public DbSet<FileAccessLog> FileAccessLogs { get; set; }
        public DbSet<FileWorkflow> FileWorkflows { get; set; }
        public DbSet<FileWorkflowRemark> FileWorkflowRemarks { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Employee Configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.BadgeNumber).IsUnique();
                entity.HasIndex(e => e.NationalId).IsUnique();
                
                entity.Property(e => e.EmploymentStatus)
                    .HasDefaultValue("Active");
                
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");
                
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");
            });
            
            // EmployeeConduct Configuration
            modelBuilder.Entity<EmployeeConduct>(entity =>
            {
                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.IncidentDate);
                entity.Property(e => e.Status).HasDefaultValue("Active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(e => e.Employee)
                    .WithMany(e => e.ConductRecords)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // EmployeePosting Configuration
            modelBuilder.Entity<EmployeePosting>(entity =>
            {
                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.StartDate);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(e => e.Employee)
                    .WithMany(e => e.Postings)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // EmployeeTraining Configuration
            modelBuilder.Entity<EmployeeTraining>(entity =>
            {
                entity.HasIndex(e => e.EmployeeId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(e => e.Employee)
                    .WithMany(e => e.Trainings)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // EmployeeAward Configuration
            modelBuilder.Entity<EmployeeAward>(entity =>
            {
                entity.HasIndex(e => e.EmployeeId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(e => e.Employee)
                    .WithMany(e => e.Awards)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // EmployeeLeave Configuration
            modelBuilder.Entity<EmployeeLeave>(entity =>
            {
                entity.HasIndex(e => e.EmployeeId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(e => e.Employee)
                    .WithMany(e => e.Leaves)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // EFile Configuration
            modelBuilder.Entity<EFile>(entity =>
            {
                entity.HasIndex(e => e.FileNumber).IsUnique();
                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.UploadDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.AccessCount).HasDefaultValue(0);
                
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // FileAccessLog Configuration
            modelBuilder.Entity<FileAccessLog>(entity =>
            {
                entity.HasIndex(e => e.FileId);
                entity.HasIndex(e => e.AccessedBy);
                entity.HasIndex(e => e.AccessTime);
                entity.Property(e => e.AccessTime).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(e => e.File)
                    .WithMany()
                    .HasForeignKey(e => e.FileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // FileWorkflow Configuration
            modelBuilder.Entity<FileWorkflow>(entity =>
            {
                entity.HasIndex(e => e.WorkflowNumber).IsUnique();
                entity.HasIndex(e => e.FileId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.FromDepartment);
                entity.HasIndex(e => e.ToDepartment);
                entity.Property(e => e.SentDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(e => e.File)
                    .WithMany()
                    .HasForeignKey(e => e.FileId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.FromEmployee)
                    .WithMany()
                    .HasForeignKey(e => e.FromEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.ToEmployee)
                    .WithMany()
                    .HasForeignKey(e => e.ToEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // FileWorkflowRemark Configuration - FIXED: No cascade delete
            modelBuilder.Entity<FileWorkflowRemark>(entity =>
            {
                entity.HasIndex(e => e.WorkflowId);
                entity.HasIndex(e => e.EmployeeId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(e => e.Workflow)
                    .WithMany()
                    .HasForeignKey(e => e.WorkflowId)
                    .OnDelete(DeleteBehavior.Restrict);  // Changed from Cascade to Restrict
                
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);  // Changed from Cascade to Restrict
            });
            
            // Seed initial data
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    EmployeeNumber = "PEN001",
                    FirstName = "John",
                    LastName = "Smith",
                    MiddleName = "Robert",
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Gender = "Male",
                    Position = "Correctional Officer",
                    Department = "Security",
                    HireDate = new DateTime(2015, 3, 10),
                    Email = "john.smith@prison.gov",
                    PhoneNumber = "(555) 123-4567",
                    EmploymentStatus = "Active",
                    SecurityClearanceLevel = "Level 3",
                    BadgeNumber = "B12345",
                    ShiftSchedule = "Day Shift",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Id = 2,
                    EmployeeNumber = "PEN002",
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    MiddleName = "Elizabeth",
                    DateOfBirth = new DateTime(1990, 8, 22),
                    Gender = "Female",
                    Position = "Administrative Officer",
                    Department = "Administration",
                    HireDate = new DateTime(2018, 6, 1),
                    Email = "sarah.johnson@prison.gov",
                    PhoneNumber = "(555) 234-5678",
                    EmploymentStatus = "Active",
                    SecurityClearanceLevel = "Level 2",
                    BadgeNumber = "B12346",
                    ShiftSchedule = "Day Shift",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = "System"
                }
            );
        }
    }
}