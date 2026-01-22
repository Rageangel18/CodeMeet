namespace CodeMeet.BLL.DTOs.Invitations;

public class AcceptInvitationRequest
{
    public string Token { get; set; } = null!;
    public Guid UserId { get; set; }
}
