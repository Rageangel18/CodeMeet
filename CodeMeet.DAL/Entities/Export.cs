public class Export
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public Guid? SessionId { get; set; }
    public string Type { get; set; } = null!;
    public string BlobUrl { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedUtc { get; set; }

    public Organization Org { get; set; } = null!;
    public Session? Session { get; set; }
    public User CreatedByUser { get; set; } = null!;
}