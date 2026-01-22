using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Sessions;

public class SessionSummaryQuestionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public QuestionLevel Level { get; set; }
    public DateTime CreatedUtc { get; set; }
}
