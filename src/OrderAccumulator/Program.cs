using System.Runtime.InteropServices;
using OrderAccumulator;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

var settings = new SessionSettings("server.cfg");

var host = Environment.GetEnvironmentVariable("FIX_SOCKET_ACCEPT_HOST");
if (!string.IsNullOrWhiteSpace(host))
{
    foreach (var sessionId in settings.GetSessions())
        settings.Get(sessionId).SetString("SocketAcceptHost", host);
}

var application = new OrderAccumulatorApplication();

var storeFactory = new FileStoreFactory(settings);
var logFactory = new FileLogFactory(settings);

using var acceptor = new ThreadedSocketAcceptor(
    application,
    storeFactory,
    settings,
    logFactory
);

using var exit = new ManualResetEventSlim();
using var sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, ctx => { ctx.Cancel = true; exit.Set(); });
using var sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx => { ctx.Cancel = true; exit.Set(); });

acceptor.Start();

Console.WriteLine("Servidor FIX iniciado.");
Console.WriteLine("Pressione Ctrl+C para encerrar.");

exit.Wait();

acceptor.Stop();
