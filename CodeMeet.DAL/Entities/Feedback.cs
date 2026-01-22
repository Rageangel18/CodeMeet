using CodeMeet.DAL.Enums;

public class Feedback
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid ReviewerUserId { get; set; }
    public Guid? CandidateParticipantId { get; set; }
    public decimal? ScoreOverall { get; set; }
    public decimal? ScoreTech { get; set; }
    public decimal? ScoreComm { get; set; }
    public Recommendation Recommendation { get; set; } = Recommendation.Hold;
    public string? Strengths { get; set; }
    public string? Concerns { get; set; }
    public DateTime CreatedUtc { get; set; }

    public Session Session { get; set; } = null!;
    public User ReviewerUser { get; set; } = null!;
    public Participant? CandidateParticipant { get; set; }
}