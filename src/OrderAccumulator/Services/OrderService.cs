
namespace OrderAccumulator.Services
{
    public class OrderService
    {
        private const decimal Limit = 100_000_000m;

        private readonly Dictionary<string, decimal> _exposures = new();

        public bool AddOrder(string symbol, decimal price, decimal qty, bool isBuy)
        {
            var currentExposure = _exposures.GetValueOrDefault(symbol);

            var financialValue = price * qty;

            var newExposure = currentExposure + (isBuy ? financialValue : -financialValue);

            if (Math.Abs(newExposure) > Limit)
            {
                Console.WriteLine($"Limite excedido para {symbol}");
                return false;
            }

            _exposures[symbol] = newExposure;
            return true;
        }

        public decimal GetExposure(string symbol)
        {
            return _exposures.GetValueOrDefault(symbol);
        }
    }
}
