using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Invitations;

public class CreateInvitationRequest
{
    public Guid SessionId { get; set; }
    public string Email { get; set; } = null!;
    public ParticipantRole Role { get; set; }
    public TimeSpan? Lifetime { get; set; } 
}
