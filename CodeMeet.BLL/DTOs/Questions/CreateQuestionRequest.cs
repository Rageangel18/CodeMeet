using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Questions;

public class CreateQuestionRequest
{
    public Guid OrgId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public QuestionLevel Level { get; set; } = QuestionLevel.Medium;
}
