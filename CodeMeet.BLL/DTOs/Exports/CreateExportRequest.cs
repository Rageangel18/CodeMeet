namespace CodeMeet.BLL.DTOs.Exports;

public class CreateExportRequest
{
    public Guid OrgId { get; set; }
    public Guid? SessionId { get; set; }
    public string Type { get; set; } = null!;
    public string BlobUrl { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
}
