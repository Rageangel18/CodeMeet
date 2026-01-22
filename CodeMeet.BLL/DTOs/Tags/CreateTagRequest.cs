namespace CodeMeet.BLL.DTOs.Tags;

public class CreateTagRequest
{
    public Guid OrgId { get; set; }
    public string Name { get; set; } = null!;
}
