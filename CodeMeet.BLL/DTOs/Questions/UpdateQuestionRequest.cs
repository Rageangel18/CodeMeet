using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Questions;

public class UpdateQuestionRequest
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public QuestionLevel? Level { get; set; }
}
