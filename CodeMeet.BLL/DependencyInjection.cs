using CodeMeet.BLL.Services;
using CodeMeet.BLL.Services.Admin;
using CodeMeet.BLL.Services.Implementations;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CodeMeet.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddCodeMeetBll(this IServiceCollection services)
    {
        // Core
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IParticipantService, ParticipantService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IAuthService, AuthService>();


        // Question bank
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<ITagService, TagService>();

        // Live coding & chat
        services.AddScoped<ISnippetService, SnippetService>();
        services.AddScoped<ISnippetCommentService, SnippetCommentService>();
        services.AddScoped<IChatService, ChatService>();

        // Feedback & reports
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<ISessionReportService, SessionReportService>();

        // Exports & audit
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IAuditLogService, AuditLogService>();


        //admin
        services.AddScoped<IAdminOrganizationService, AdminOrganizationService>();
        services.AddScoped<IAdminUserService, AdminUserService>();

        //rabbit-runner-exec
        services.AddScoped<IExecRequestService, ExecRequestService>();
        services.AddScoped<IExecOrchestrator, ExecOrchestrator>();


        return services;
    }
}
