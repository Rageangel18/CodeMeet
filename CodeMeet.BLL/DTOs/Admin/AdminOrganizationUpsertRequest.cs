namespace CodeMeet.BLL.Dtos.Admin;

public sealed class AdminOrganizationUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int DataRetentionDays { get; set; } = 365;
}
