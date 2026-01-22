using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeMeet.BLL.DTOs.Exec;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL.Enums;
using CodeMeet.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using BLL.Messaging;
using CodeMeet.Contracts.Exec.Messages;

namespace CodeMeet.Messaging;

public sealed class ExecCompletedConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() } // чтобы "Completed" парсилось в enum
    };

    private readonly ILogger<ExecCompletedConsumer> _log;
    private readonly RabbitMqOptions _opt;
    private readonly IConnection _conn;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<SessionHub> _hub;

    private IChannel? _ch;

    public ExecCompletedConsumer(
        ILogger<ExecCompletedConsumer> log,
        IOptions<RabbitMqOptions> opt,
        IConnection conn,
        IServiceScopeFactory scopeFactory,
        IHubContext<SessionHub> hub)
    {
        _log = log;
        _opt = opt.Value;
        _conn = conn;
        _scopeFactory = scopeFactory;
        _hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ch = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);

        await _ch.BasicQosAsync(0, 5, false, stoppingToken);

        await _ch.QueueDeclareAsync(
            queue: _opt.QueueCompleted,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_ch);
        consumer.ReceivedAsync += OnMessageAsync;

        await _ch.BasicConsumeAsync(
            queue: _opt.QueueCompleted,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _log.LogInformation("ExecCompletedConsumer started. queue={Queue}", _opt.QueueCompleted);
    }

    private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        if (_ch is null) return;

        var correlationId = ea.BasicProperties?.CorrelationId;
        var json = string.Empty;

        try
        {
            json = Encoding.UTF8.GetString(ea.Body.ToArray());

            var msg = JsonSerializer.Deserialize<ExecRunCompletedMessage>(json, JsonOpts)
                      ?? throw new InvalidOperationException("Message is null");

            using var scope = _scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IExecRequestService>();

            var req = new CompleteExecRequestRequest
            {
                Id = msg.ExecRequestId,
                Status = msg.Status == Contracts.Exec.Messages.ExecRunResultStatus.Completed ? ExecStatus.Completed : ExecStatus.Failed,
                ExitCode = msg.ExitCode,
                Stdout = msg.Stdout,
                Stderr = msg.Stderr,
                DurationMs = msg.DurationMs,
                Truncated = msg.Truncated
            };

            await svc.CompleteAsync(req);

            var dto = await svc.GetByIdAsync(msg.ExecRequestId);
            if (dto is not null)
                await _hub.NotifyRunCompletedAsync(dto.SessionId, dto);

            await _ch.BasicAckAsync(ea.DeliveryTag, false);

            _log.LogInformation(
                "exec.completed processed ok. execRequestId={Id} status={Status} exitCode={ExitCode} correlationId={CorrelationId}",
                msg.ExecRequestId, req.Status, req.ExitCode, correlationId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed processing exec.completed. correlationId={CorrelationId} json={Json}", correlationId, json);
            await _ch.BasicRejectAsync(ea.DeliveryTag, requeue: false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_ch is not null)
        {
            await _ch.CloseAsync(cancellationToken);
            await _ch.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
