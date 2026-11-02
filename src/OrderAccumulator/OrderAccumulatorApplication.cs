using OrderAccumulator.Services;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace OrderAccumulator
{
    public class OrderAccumulatorApplication : MessageCracker, IApplication
    {
        private readonly OrderService _orderService;

        public OrderAccumulatorApplication()
        {
            _orderService = new OrderService();
        }

        public void OnCreate(SessionID sessionID)
        {
            Console.WriteLine($"[CREATE] {sessionID}");
        }

        public void OnLogon(SessionID sessionID)
        {
            Console.WriteLine($"[LOGON] Cliente conectado: {sessionID}");
        }

        public void OnLogout(SessionID sessionID)
        {
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

        public void OnMessage(NewOrderSingle order, SessionID sessionID)
        {
            Console.WriteLine("Ordem recebida");

            Session.SendToTarget(ProcessOrder(order), sessionID);
        }

        public ExecutionReport ProcessOrder(NewOrderSingle order)
        {
            var symbol = order.Symbol.Value;
            var price = order.Price.Value;
            var quantity = order.OrderQty.Value;
            bool isBuy = order.Side.Value == '1';

            var accepted = _orderService.AddOrder(symbol, price, quantity, isBuy);

            var report = new ExecutionReport(
                aOrderID: new OrderID(Guid.NewGuid().ToString("N")),
                aExecID: new ExecID(Guid.NewGuid().ToString("N")),
                aExecType: new ExecType(accepted
                    ? ExecType.NEW
                    : ExecType.REJECTED),
                aOrdStatus: new OrdStatus(accepted
                    ? OrdStatus.NEW
                    : OrdStatus.REJECTED),
                aSide: new Side(order.Side.Value),
                aLeavesQty: new LeavesQty(accepted ? quantity : 0m),
                aCumQty: new CumQty(0m),
                aAvgPx: new AvgPx(0m),
                aSymbol: new Symbol(symbol)
            );

            report.SetField(new ClOrdID(order.ClOrdID.Value));
            report.SetField(new OrderQty(quantity));
            report.SetField(new OrdType(order.OrdType.Value));
            report.SetField(new TransactTime(DateTime.UtcNow));
            report.SetField(new Price(price));

            if (!accepted)
                report.SetField(new Text("Exposure limit exceeded"));

            return report;
        }
    }
}
