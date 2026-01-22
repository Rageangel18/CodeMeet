using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Participants;

public class ParticipantDto
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

    // invited/accepted/joined/left/no_show
    public string Status { get; set; } = null!;

    public bool IsGuest { get; set; }
    public DateTime CreatedUtc { get; set; }
}
