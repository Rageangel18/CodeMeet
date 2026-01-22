using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Questions;

public class QuestionDto
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public QuestionLevel Level { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public IReadOnlyCollection<Guid>? TagIds { get; set; }
}
