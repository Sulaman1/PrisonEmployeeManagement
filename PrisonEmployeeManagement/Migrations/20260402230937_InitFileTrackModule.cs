using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PrisonEmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitFileTrackModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetirementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AlternativePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactRelationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmploymentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    EmployeeType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SecurityClearanceLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ClearanceExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BadgeNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShiftSchedule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Qualifications = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Skills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Languages = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BloodType = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAwards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    AwardName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AwardDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PresentedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MonetaryValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAwards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAwards_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeConducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ConductType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ActionTaken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisciplinaryAction = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AwardType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CertificateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Witnesses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupportingDocuments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    ResolutionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeConducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeConducts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLeaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDays = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLeaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaves_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePostings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FacilityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RankAtPosting = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    PostingType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TransferReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePostings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePostings_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTrainings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    TrainingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrainingType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationHours = table.Column<int>(type: "int", nullable: true),
                    CertificateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CertificateExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Score = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTrainings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeTrainings_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    ConductId = table.Column<int>(type: "int", nullable: true),
                    PostingId = table.Column<int>(type: "int", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConfidentialLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastAccessed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccessCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFiles_EmployeeConducts_ConductId",
                        column: x => x.ConductId,
                        principalTable: "EmployeeConducts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EFiles_EmployeePostings_PostingId",
                        column: x => x.PostingId,
                        principalTable: "EmployeePostings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EFiles_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileAccessLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<int>(type: "int", nullable: false),
                    AccessedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccessTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileAccessLogs_EFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "EFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileWorkflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<int>(type: "int", nullable: false),
                    WorkflowNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FromDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ToDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FromEmployeeId = table.Column<int>(type: "int", nullable: false),
                    ToEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileWorkflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileWorkflows_EFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "EFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileWorkflows_Employees_FromEmployeeId",
                        column: x => x.FromEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileWorkflows_Employees_ToEmployeeId",
                        column: x => x.ToEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileWorkflowRemarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileWorkflowRemarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileWorkflowRemarks_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileWorkflowRemarks_FileWorkflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "FileWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Address", "AlternativePhone", "BadgeNumber", "BloodType", "City", "ClearanceExpiryDate", "ConfirmationDate", "Country", "CreatedAt", "CreatedBy", "DateOfBirth", "Department", "Email", "EmergencyContactName", "EmergencyContactPhone", "EmergencyContactRelationship", "EmployeeNumber", "EmployeeType", "EmploymentStatus", "FirstName", "Gender", "HireDate", "Languages", "LastModifiedBy", "LastName", "MedicalConditions", "MiddleName", "NationalId", "PassportNumber", "PhoneNumber", "Position", "Qualifications", "Rank", "RetirementDate", "SecurityClearanceLevel", "ShiftSchedule", "Skills", "State", "UpdatedAt", "ZipCode" },
                values: new object[,]
                {
                    { 1, null, null, "B12345", null, null, null, null, "USA", new DateTime(2026, 4, 3, 4, 9, 35, 672, DateTimeKind.Local).AddTicks(2508), "System", new DateTime(1985, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Security", "john.smith@prison.gov", null, null, null, "PEN001", "Permanent", "Active", "John", "Male", new DateTime(2015, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Smith", null, "Robert", null, null, "(555) 123-4567", "Correctional Officer", null, null, null, "Level 3", "Day Shift", null, null, new DateTime(2026, 4, 3, 4, 9, 35, 672, DateTimeKind.Local).AddTicks(2645), null },
                    { 2, null, null, "B12346", null, null, null, null, "USA", new DateTime(2026, 4, 3, 4, 9, 35, 672, DateTimeKind.Local).AddTicks(2672), "System", new DateTime(1990, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Administration", "sarah.johnson@prison.gov", null, null, null, "PEN002", "Permanent", "Active", "Sarah", "Female", new DateTime(2018, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Johnson", null, "Elizabeth", null, null, "(555) 234-5678", "Administrative Officer", null, null, null, "Level 2", "Day Shift", null, null, new DateTime(2026, 4, 3, 4, 9, 35, 672, DateTimeKind.Local).AddTicks(2682), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EFiles_Category",
                table: "EFiles",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_EFiles_ConductId",
                table: "EFiles",
                column: "ConductId");

            migrationBuilder.CreateIndex(
                name: "IX_EFiles_EmployeeId",
                table: "EFiles",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EFiles_FileNumber",
                table: "EFiles",
                column: "FileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EFiles_PostingId",
                table: "EFiles",
                column: "PostingId");

            migrationBuilder.CreateIndex(
                name: "IX_EFiles_Status",
                table: "EFiles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAwards_EmployeeId",
                table: "EmployeeAwards",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeConducts_EmployeeId",
                table: "EmployeeConducts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeConducts_IncidentDate",
                table: "EmployeeConducts",
                column: "IncidentDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaves_EmployeeId",
                table: "EmployeeLeaves",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePostings_EmployeeId",
                table: "EmployeePostings",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePostings_StartDate",
                table: "EmployeePostings",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BadgeNumber",
                table: "Employees",
                column: "BadgeNumber",
                unique: true,
                filter: "[BadgeNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_NationalId",
                table: "Employees",
                column: "NationalId",
                unique: true,
                filter: "[NationalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTrainings_EmployeeId",
                table: "EmployeeTrainings",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FileAccessLogs_AccessedBy",
                table: "FileAccessLogs",
                column: "AccessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FileAccessLogs_AccessTime",
                table: "FileAccessLogs",
                column: "AccessTime");

            migrationBuilder.CreateIndex(
                name: "IX_FileAccessLogs_FileId",
                table: "FileAccessLogs",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_FileWorkflowRemarks_EmployeeId",
                table: "FileWorkflowRemarks",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FileWorkflowRemarks_WorkflowId",
                table: "FileWorkflowRemarks",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_FileWorkflows_FileId",
                table: "FileWorkflows",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_FileWorkflows_FromDepartment",
                table: "FileWorkflows",
                column: "FromDepartment");

            migrationBuilder.CreateIndex(
                name: "IX_FileWorkflows_FromEmployeeId",
                table: "FileWorkflows",
                column: "FromEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FileWorkflows_Status",
                table: "FileWorkflows",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FileWorkflows_ToDepartment",
                table: "FileWorkflows",
                column: "ToDepartment");

            migrationBuilder.CreateIndex(
                name: "IX_FileWorkflows_ToEmployeeId",
                table: "FileWorkflows",
                column: "ToEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FileWorkflows_WorkflowNumber",
                table: "FileWorkflows",
                column: "WorkflowNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeAwards");

            migrationBuilder.DropTable(
                name: "EmployeeLeaves");

            migrationBuilder.DropTable(
                name: "EmployeeTrainings");

            migrationBuilder.DropTable(
                name: "FileAccessLogs");

            migrationBuilder.DropTable(
                name: "FileWorkflowRemarks");

            migrationBuilder.DropTable(
                name: "FileWorkflows");

            migrationBuilder.DropTable(
                name: "EFiles");

            migrationBuilder.DropTable(
                name: "EmployeeConducts");

            migrationBuilder.DropTable(
                name: "EmployeePostings");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
