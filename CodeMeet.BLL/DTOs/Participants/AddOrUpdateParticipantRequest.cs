using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Participants;

public class AddOrUpdateParticipantRequest
{
    public Guid SessionId { get; set; }
    public Guid? UserId { get; set; }      // для внутренних пользователей
    public string? Email { get; set; }     // для гостя/кандидата
    public ParticipantRole Role { get; set; }

    public string? DisplayName { get; set; }
}
