using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Feedback;

public class FeedbackDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid ReviewerUserId { get; set; }
    public Guid? CandidateParticipantId { get; set; }

    public decimal? ScoreOverall { get; set; }
    public decimal? ScoreTech { get; set; }
    public decimal? ScoreComm { get; set; }

    public Recommendation Recommendation { get; set; }
    public string? Strengths { get; set; }
    public string? Concerns { get; set; }

    public DateTime CreatedUtc { get; set; }
}
