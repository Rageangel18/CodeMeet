public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int DataRetentionDays { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }

    public ICollection<OrganizationUser> Members { get; set; } = new List<OrganizationUser>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<Export> Exports { get; set; } = new List<Export>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}