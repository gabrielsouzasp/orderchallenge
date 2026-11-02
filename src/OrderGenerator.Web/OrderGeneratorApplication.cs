using System.Collections.Concurrent;
using QuickFix;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace OrderGenerator
{
    public class OrderGeneratorApplication : MessageCracker, IApplication
    {
        private SessionID? _sessionId;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<ExecutionReport>> _pending = new();

        public bool IsConnected => _sessionId is not null &&
            Session.LookupSession(_sessionId)?.IsLoggedOn == true;


        public void OnCreate(SessionID sessionID)
        {
            Console.WriteLine($"[CREATE] {sessionID}");
        }

        public void OnLogon(SessionID sessionID)
        {
            _sessionId = sessionID;
            Console.WriteLine($"[LOGON] Cliente conectado: {sessionID}");
        }

        public void OnLogout(SessionID sessionID)
        {
            if (_sessionId?.Equals(sessionID) == true)
                _sessionId = null;

            Console.WriteLine($"[LOGOUT] Cliente desconectado: {sessionID}");
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine($"[ADMIN IN] {message}");
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine($"[ADMIN OUT] {message}");
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            Console.WriteLine($"[APP IN] {message}");
            Crack(message, sessionID);
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            Console.WriteLine($"[APP OUT] {message}");
        }

        public void OnMessage(ExecutionReport report, SessionID sessionID)
        {
            if (_pending.TryRemove(report.ClOrdID.Value, out var tcs))
                tcs.TrySetResult(report);
        }

        public async Task<ExecutionReport> SendOrderAsync(NewOrderSingle order, TimeSpan timeout)
        {
            var clOrdId = order.ClOrdID.Value;

            var tcs = new TaskCompletionSource<ExecutionReport>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[clOrdId] = tcs;

            try
            {
                Send(order);
                return await tcs.Task.WaitAsync(timeout);
            }
            finally
            {
                _pending.TryRemove(clOrdId, out _);
            }
        }

        public void Send(Message message)
        {
            var sessionId = _sessionId;

            if (sessionId is null ||
                Session.LookupSession(sessionId)?.IsLoggedOn != true)
            {
                throw new InvalidOperationException(
                    "Sessão FIX não conectada."
                );
            }

            if (!Session.SendToTarget(message, sessionId))
                throw new InvalidOperationException(
                    "Falha ao enviar mensagem FIX."
                );
        }
    }
}
