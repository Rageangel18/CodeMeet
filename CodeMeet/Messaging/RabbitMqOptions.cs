namespace CodeMeet.Messaging;

public sealed class RabbitMqOptions
{
    public string Host { get; init; } = "4.235.105.188";
    public int Port { get; init; } = 5672;
    public string User { get; init; } = "codemeet";
    public string Pass { get; init; } = "alpha123";

    public string QueueRun { get; init; } = "exec.run";
    public string QueueCompleted { get; init; } = "exec.completed";

    public ushort PrefetchCount { get; init; } = 5;
}
