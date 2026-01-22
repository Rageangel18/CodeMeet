using CodeMeet.DAL.Enums;

public class Participant
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? UserId { get; set; }
    public ParticipantRole Role { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public DateTime? InvitedUtc { get; set; }
    public DateTime? JoinedUtc { get; set; }
    public DateTime? LeftUtc { get; set; }
    public string? Status { get; set; }
    public bool IsGuest { get; set; }
    public DateTime CreatedUtc { get; set; }

    public Session Session { get; set; } = null!;
    public User? User { get; set; }

    public ICollection<Feedback> FeedbacksAsCandidate { get; set; } = new List<Feedback>();
}