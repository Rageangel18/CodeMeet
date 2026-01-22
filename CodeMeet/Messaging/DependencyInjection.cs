using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.Web.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CodeMeet.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddExecRabbitMessaging(this IServiceCollection services, IConfiguration cfg)
    {
        services.Configure<RabbitMqOptions>(cfg.GetSection("Rabbit"));

        services.AddSingleton<IConnection>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            return RabbitMqConnectionFactory.Create(opt);
        });

        services.AddSingleton<IExecRunPublisher, ExecRunPublisher>();

        services.AddHostedService<ExecCompletedConsumer>();



        return services;
    }
}
