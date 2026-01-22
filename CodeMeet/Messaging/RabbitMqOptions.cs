namespace CodeMeet.Messaging;

public sealed class RabbitMqOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string User { get; init; } = "guest";
    public string Pass { get; init; } = "guest";

    public string QueueRun { get; init; } = "exec.run";
    public string QueueCompleted { get; init; } = "exec.completed";

    public ushort PrefetchCount { get; init; } = 5;
}
