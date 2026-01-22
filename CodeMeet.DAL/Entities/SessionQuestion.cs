public class SessionQuestion
{
    public Guid SessionId { get; set; }
    public Guid QuestionId { get; set; }
    public int OrderIndex { get; set; }

    public Session Session { get; set; } = null!;
    public Question Question { get; set; } = null!;
}