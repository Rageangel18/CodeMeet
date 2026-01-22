namespace CodeMeet.BLL.DTOs.Organizations;

public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int DataRetentionDays { get; set; }
    public DateTime CreatedUtc { get; set; }
}
