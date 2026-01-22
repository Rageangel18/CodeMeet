namespace CodeMeet.BLL.DTOs.Tags;

public class TagDto
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedUtc { get; set; }
}
