using System.Text;
using System.Text.Json;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.Contracts.Exec.Messages;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CodeMeet.Messaging;

public sealed class ExecRunPublisher : IExecRunPublisher
{
    private readonly IConnection _conn;
    private readonly RabbitMqOptions _opt;

    public ExecRunPublisher(IConnection conn, IOptions<RabbitMqOptions> opt)
    {
        _conn = conn;
        _opt = opt.Value;
    }

    public async Task PublishRunAsync(ExecRunRequestedMessage message, CancellationToken ct = default)
    {
        await using var ch = await _conn.CreateChannelAsync(cancellationToken: ct);

        await ch.QueueDeclareAsync(
            queue: _opt.QueueRun,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            CorrelationId = message.ExecRequestId.ToString()
        };

        await ch.BasicPublishAsync(
            exchange: "",
            routingKey: _opt.QueueRun,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);
    }
}
