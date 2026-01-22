using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeMeet.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DataRetentionDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 365),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    PasswordSalt = table.Column<string>(type: "text", nullable: true),
                    PasswordCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tags_organizations_OrgId",
                        column: x => x.OrgId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_users",
                columns: table => new
                {
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_users", x => new { x.OrgId, x.UserId });
                    table.ForeignKey(
                        name: "FK_organization_users_organizations_OrgId",
                        column: x => x.OrgId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_users_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_questions_organizations_OrgId",
                        column: x => x.OrgId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_questions_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "Draft"),
                    ScheduledStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsExecEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DefaultLanguage = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "CSharp"),
                    RecordingBlobUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sessions_organizations_OrgId",
                        column: x => x.OrgId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sessions_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "question_tags",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_tags", x => new { x.QuestionId, x.TagId });
                    table.ForeignKey(
                        name: "FK_question_tags_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_question_tags_tags_TagId",
                        column: x => x.TagId,
                        principalTable: "tags",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_log_organizations_OrgId",
                        column: x => x.OrgId,
                        principalTable: "organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_audit_log_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_audit_log_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chat_messages_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chat_messages_users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "exports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BlobUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exports_organizations_OrgId",
                        column: x => x.OrgId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exports_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_exports_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcceptedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcceptedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invitations_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invitations_users_AcceptedUserId",
                        column: x => x.AcceptedUserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    InvitedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JoinedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LeftUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IsGuest = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_participants_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_participants_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "session_questions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_questions", x => new { x.SessionId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_session_questions_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "questions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_session_questions_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "snippets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Language = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "CSharp"),
                    Filename = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsFinalSolution = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_snippets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_snippets_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_snippets_users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "feedback",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateParticipantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScoreOverall = table.Column<decimal>(type: "numeric(3,1)", nullable: true),
                    ScoreTech = table.Column<decimal>(type: "numeric(3,1)", nullable: true),
                    ScoreComm = table.Column<decimal>(type: "numeric(3,1)", nullable: true),
                    Recommendation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Strengths = table.Column<string>(type: "text", nullable: true),
                    Concerns = table.Column<string>(type: "text", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_feedback_participants_CandidateParticipantId",
                        column: x => x.CandidateParticipantId,
                        principalTable: "participants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_feedback_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_feedback_users_ReviewerUserId",
                        column: x => x.ReviewerUserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "exec_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnippetId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Language = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "Queued"),
                    ExitCode = table.Column<int>(type: "integer", nullable: true),
                    Stdout = table.Column<string>(type: "text", nullable: true),
                    Stderr = table.Column<string>(type: "text", nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    Truncated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exec_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exec_requests_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exec_requests_snippets_SnippetId",
                        column: x => x.SnippetId,
                        principalTable: "snippets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_exec_requests_users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "snippet_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SnippetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LineStart = table.Column<int>(type: "integer", nullable: true),
                    LineEnd = table.Column<int>(type: "integer", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_snippet_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_snippet_comments_snippets_SnippetId",
                        column: x => x.SnippetId,
                        principalTable: "snippets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_snippet_comments_users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_OrgId_CreatedUtc",
                table: "audit_log",
                columns: new[] { "OrgId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_SessionId_CreatedUtc",
                table: "audit_log",
                columns: new[] { "SessionId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_UserId_CreatedUtc",
                table: "audit_log",
                columns: new[] { "UserId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_AuthorUserId",
                table: "chat_messages",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_SessionId_CreatedUtc",
                table: "chat_messages",
                columns: new[] { "SessionId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_exec_requests_RequestedByUserId",
                table: "exec_requests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_exec_requests_SessionId_CreatedUtc",
                table: "exec_requests",
                columns: new[] { "SessionId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_exec_requests_SessionId_Status",
                table: "exec_requests",
                columns: new[] { "SessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_exec_requests_SnippetId",
                table: "exec_requests",
                column: "SnippetId");

            migrationBuilder.CreateIndex(
                name: "IX_exports_CreatedByUserId",
                table: "exports",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_exports_OrgId_CreatedUtc",
                table: "exports",
                columns: new[] { "OrgId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_exports_SessionId_CreatedUtc",
                table: "exports",
                columns: new[] { "SessionId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_feedback_CandidateParticipantId",
                table: "feedback",
                column: "CandidateParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_ReviewerUserId",
                table: "feedback",
                column: "ReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_SessionId_CreatedUtc",
                table: "feedback",
                columns: new[] { "SessionId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_invitations_AcceptedUserId",
                table: "invitations",
                column: "AcceptedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_SessionId_Email",
                table: "invitations",
                columns: new[] { "SessionId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_users_OrgId_Role",
                table: "organization_users",
                columns: new[] { "OrgId", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_organization_users_UserId",
                table: "organization_users",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_participants_Email",
                table: "participants",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_participants_SessionId_CreatedUtc",
                table: "participants",
                columns: new[] { "SessionId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_participants_SessionId_Role",
                table: "participants",
                columns: new[] { "SessionId", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_participants_UserId",
                table: "participants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_question_tags_TagId",
                table: "question_tags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_questions_CreatedByUserId",
                table: "questions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_questions_OrgId_CreatedUtc",
                table: "questions",
                columns: new[] { "OrgId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_questions_OrgId_Level",
                table: "questions",
                columns: new[] { "OrgId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_session_questions_QuestionId",
                table: "session_questions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_session_questions_SessionId_OrderIndex",
                table: "session_questions",
                columns: new[] { "SessionId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_CreatedByUserId",
                table: "sessions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_OrgId_Status",
                table: "sessions",
                columns: new[] { "OrgId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_ScheduledStartUtc",
                table: "sessions",
                column: "ScheduledStartUtc");

            migrationBuilder.CreateIndex(
                name: "IX_snippet_comments_AuthorUserId",
                table: "snippet_comments",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_snippet_comments_SnippetId_CreatedUtc",
                table: "snippet_comments",
                columns: new[] { "SnippetId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_snippets_AuthorUserId",
                table: "snippets",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_snippets_SessionId_CreatedUtc",
                table: "snippets",
                columns: new[] { "SessionId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_snippets_SessionId_IsFinalSolution",
                table: "snippets",
                columns: new[] { "SessionId", "IsFinalSolution" });

            migrationBuilder.CreateIndex(
                name: "IX_tags_OrgId_Name",
                table: "tags",
                columns: new[] { "OrgId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "exec_requests");

            migrationBuilder.DropTable(
                name: "exports");

            migrationBuilder.DropTable(
                name: "feedback");

            migrationBuilder.DropTable(
                name: "invitations");

            migrationBuilder.DropTable(
                name: "organization_users");

            migrationBuilder.DropTable(
                name: "question_tags");

            migrationBuilder.DropTable(
                name: "session_questions");

            migrationBuilder.DropTable(
                name: "snippet_comments");

            migrationBuilder.DropTable(
                name: "participants");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "snippets");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
