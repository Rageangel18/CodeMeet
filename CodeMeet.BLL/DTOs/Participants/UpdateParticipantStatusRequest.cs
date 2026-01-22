namespace CodeMeet.BLL.DTOs.Participants;

public class UpdateParticipantStatusRequest
{
    public Guid ParticipantId { get; set; }
    public string Status { get; set; } = null!; // invited/accepted/joined/left/no_show
    public bool SetJoinedNow { get; set; }      // удобные флажки
    public bool SetLeftNow { get; set; }
}
