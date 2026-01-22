public class Tag
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedUtc { get; set; }

    public Organization Org { get; set; } = null!;
    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
}
