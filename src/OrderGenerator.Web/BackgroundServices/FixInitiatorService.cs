using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace OrderGenerator.BackgroundServices
{
    public class FixInitiatorService : IHostedService, IDisposable
    {
        private readonly SocketInitiator _socketInitiator;

        public FixInitiatorService(OrderGeneratorApplication orderGeneratorApplication)
        {
            var settings = new SessionSettings("client.cfg");

            var host = Environment.GetEnvironmentVariable("FIX_SOCKET_CONNECT_HOST");
            if (!string.IsNullOrWhiteSpace(host))
            {
                foreach (var sessionId in settings.GetSessions())
                    settings.Get(sessionId).SetString("SocketConnectHost", host);
            }

            _socketInitiator = new SocketInitiator(
                orderGeneratorApplication,
                new FileStoreFactory(settings),
                settings,
                new FileLogFactory(settings)
            );
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _socketInitiator.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _socketInitiator.Stop();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _socketInitiator.Dispose();
        }
    }
}
