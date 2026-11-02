using OrderAccumulator;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderChallenge.Tests
{
    public class OrderAccumulatorApplicationTests
    {
        private readonly OrderAccumulatorApplication _app = new();

        private static NewOrderSingle CreateOrder(
            string clOrdId, string symbol, char side, decimal quantity, decimal price)
        {
            return new NewOrderSingle
            {
                ClOrdID = new ClOrdID(clOrdId),
                Symbol = new Symbol(symbol),
                Side = new Side(side),
                TransactTime = new TransactTime(DateTime.UtcNow),
                OrderQty = new OrderQty(quantity),
                Price = new Price(price),
                OrdType = new OrdType(OrdType.LIMIT)
            };
        }

        [Fact]
        public void AcceptedOrder_ReturnsExecTypeNew()
        {
            var report = _app.ProcessOrder(CreateOrder("1", "PETR4", Side.BUY, 100m, 10m));

            Assert.Equal(ExecType.NEW, report.ExecType.Value);
            Assert.Equal(OrdStatus.NEW, report.OrdStatus.Value);
            Assert.Equal(100m, report.LeavesQty.Value);
            Assert.False(report.IsSetText());
        }

        [Fact]
        public void Report_EchoesOrderFields()
        {
            var report = _app.ProcessOrder(CreateOrder("abc", "VALE3", Side.SELL, 50m, 12.34m));

            Assert.Equal("abc", report.ClOrdID.Value);
            Assert.Equal("VALE3", report.Symbol.Value);
            Assert.Equal(Side.SELL, report.Side.Value);
            Assert.Equal(50m, report.OrderQty.Value);
            Assert.Equal(12.34m, report.Price.Value);
            Assert.Equal(OrdType.LIMIT, report.OrdType.Value);
            Assert.False(string.IsNullOrEmpty(report.OrderID.Value));
            Assert.False(string.IsNullOrEmpty(report.ExecID.Value));
        }

        [Fact]
        public void OrderExceedingLimit_ReturnsExecTypeRejected()
        {
            _app.ProcessOrder(CreateOrder("1", "PETR4", Side.BUY, 99_999m, 999.99m));

            var report = _app.ProcessOrder(CreateOrder("2", "PETR4", Side.BUY, 99_999m, 999.99m));

            Assert.Equal(ExecType.REJECTED, report.ExecType.Value);
            Assert.Equal(OrdStatus.REJECTED, report.OrdStatus.Value);
            Assert.Equal(0m, report.LeavesQty.Value);
            Assert.Equal("Exposure limit exceeded", report.Text.Value);
        }

        [Fact]
        public void RejectedOrder_IsNotCountedInExposure()
        {
            _app.ProcessOrder(CreateOrder("1", "PETR4", Side.BUY, 99_999m, 999.99m));
            _app.ProcessOrder(CreateOrder("2", "PETR4", Side.BUY, 99_999m, 999.99m));

            _app.ProcessOrder(CreateOrder("3", "PETR4", Side.SELL, 99_999m, 999.99m));
            var report = _app.ProcessOrder(CreateOrder("4", "PETR4", Side.BUY, 99_999m, 999.99m));

            Assert.Equal(ExecType.NEW, report.ExecType.Value);
        }

        [Fact]
        public void SellOrders_DecreaseExposure()
        {
            _app.ProcessOrder(CreateOrder("1", "PETR4", Side.SELL, 99_999m, 999.99m));

            var report = _app.ProcessOrder(CreateOrder("2", "PETR4", Side.SELL, 99_999m, 999.99m));

            Assert.Equal(ExecType.REJECTED, report.ExecType.Value);
        }

        [Fact]
        public void Limit_IsPerSymbol()
        {
            _app.ProcessOrder(CreateOrder("1", "PETR4", Side.BUY, 99_999m, 999.99m));

            var report = _app.ProcessOrder(CreateOrder("2", "VIIA4", Side.BUY, 99_999m, 999.99m));

            Assert.Equal(ExecType.NEW, report.ExecType.Value);
        }
    }
}
