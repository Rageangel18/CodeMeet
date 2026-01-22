using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Invitations;

public class InvitationDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Email { get; set; } = null!;
    public ParticipantRole Role { get; set; }

    public string Token { get; set; } = null!;
    public DateTime ExpiresUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? SentUtc { get; set; }

    public Guid? AcceptedUserId { get; set; }
    public DateTime? AcceptedUtc { get; set; }
}
