namespace CodeMeet.BLL.DTOs.Exports;

public class ExportDto
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public Guid? SessionId { get; set; }
    public string Type { get; set; } = null!; // csv|pdf|json
    public string BlobUrl { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedUtc { get; set; }
}
