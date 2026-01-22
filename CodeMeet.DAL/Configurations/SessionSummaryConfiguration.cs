using CodeMeet.DAL.QueryModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeMeet.DAL.Configurations;

public class SessionSummaryConfiguration : IEntityTypeConfiguration<SessionSummary>
{
    public void Configure(EntityTypeBuilder<SessionSummary> entity)
    {
        entity.HasNoKey();
        entity.ToView(null);

        entity.Property(x => x.SessionId).HasColumnName("session_id");
        entity.Property(x => x.OrgId).HasColumnName("org_id");
        entity.Property(x => x.OrgName).HasColumnName("org_name");
        entity.Property(x => x.Title).HasColumnName("title");
        entity.Property(x => x.Status).HasColumnName("status");

        entity.Property(x => x.ScheduledStartUtc).HasColumnName("scheduled_start_utc");
        entity.Property(x => x.ScheduledEndUtc).HasColumnName("scheduled_end_utc");
        entity.Property(x => x.CreatedUtc).HasColumnName("created_utc");

        entity.Property(x => x.CandidateCount).HasColumnName("candidate_count");
        entity.Property(x => x.InterviewerCount).HasColumnName("interviewer_count");
        entity.Property(x => x.ObserverCount).HasColumnName("observer_count");

        entity.Property(x => x.SnippetCount).HasColumnName("snippet_count");
        entity.Property(x => x.ExecRequestCount).HasColumnName("exec_request_count");

        entity.Property(x => x.AvgScoreOverall).HasColumnName("avg_score_overall");
        entity.Property(x => x.AvgScoreTech).HasColumnName("avg_score_tech");
        entity.Property(x => x.AvgScoreComm).HasColumnName("avg_score_comm");

        entity.Property(x => x.StrengthsSummary).HasColumnName("strengths_summary");
        entity.Property(x => x.ConcernsSummary).HasColumnName("concerns_summary");

        entity.Property(x => x.QuestionsJson).HasColumnName("questions_json");
    }
}
