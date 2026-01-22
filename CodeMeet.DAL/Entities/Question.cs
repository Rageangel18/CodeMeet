using CodeMeet.DAL.Enums;

public class Question
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public QuestionLevel Level { get; set; } = QuestionLevel.Medium;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }

    public Organization Org { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    public ICollection<SessionQuestion> SessionQuestions { get; set; } = new List<SessionQuestion>();
}