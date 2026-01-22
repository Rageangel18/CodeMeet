using RabbitMQ.Client;

namespace CodeMeet.Messaging;

public static class RabbitMqConnectionFactory
{
    public static IConnection Create(RabbitMqOptions opt)
    {
        var factory = new ConnectionFactory
        {
            HostName = opt.Host,
            Port = opt.Port,
            UserName = opt.User,
            Password = opt.Pass,

            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),

            RequestedHeartbeat = TimeSpan.FromSeconds(30),
            ClientProvidedName = "CodeMeet.MainApp"
        };

        Exception? last = null;

        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                last = ex;
                var delayMs = Math.Min(15000, attempt * 1000);
                Thread.Sleep(delayMs);
            }
        }

        throw new InvalidOperationException(
            $"RabbitMQ connection failed after retries. Host={opt.Host}:{opt.Port}",
            last);
    }
}
