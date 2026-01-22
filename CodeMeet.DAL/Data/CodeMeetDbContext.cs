using CodeMeet.DAL.QueryModels;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.DAL;

public class CodeMeetDbContext : DbContext
{
    public CodeMeetDbContext(DbContextOptions<CodeMeetDbContext> options)
        : base(options)
    {
    }

    // Core: organizations & users
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OrganizationUser> OrganizationUsers => Set<OrganizationUser>();

    // Sessions & participants
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Invitation> Invitations => Set<Invitation>();

    // Question bank
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<QuestionTag> QuestionTags => Set<QuestionTag>();
    public DbSet<SessionQuestion> SessionQuestions => Set<SessionQuestion>();

    // Live coding & chat
    public DbSet<Snippet> Snippets => Set<Snippet>();
    public DbSet<SnippetComment> SnippetComments => Set<SnippetComment>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public DbSet<ExecRequest> ExecRequests => Set<ExecRequest>();

    // Feedback, exports, audit
    public DbSet<Feedback> Feedback => Set<Feedback>();
    public DbSet<Export> Exports => Set<Export>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Read model для sp_GetSessionSummary
    public DbSet<SessionSummary> SessionSummaries => Set<SessionSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CodeMeetDbContext).Assembly);
    }
}
