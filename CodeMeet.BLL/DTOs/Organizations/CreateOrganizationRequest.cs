namespace CodeMeet.BLL.DTOs.Organizations;

public class CreateOrganizationRequest
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int? DataRetentionDays { get; set; }
}
