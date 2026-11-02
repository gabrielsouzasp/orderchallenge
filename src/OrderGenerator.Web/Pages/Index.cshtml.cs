using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickFix.FIX44;
using System.ComponentModel.DataAnnotations;
using QuickFix.Fields;

namespace OrderGenerator.Web.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public OrderInput Order { get; set; } = new();

        public string? Message { get; set; }

        public bool? Accepted { get; set; }

        private string[] AcceptedSymbols = ["PETR4", "VALE3", "VIIA4"];

        private readonly OrderGeneratorApplication _orderGeneratorApplication;

        public IndexModel(OrderGeneratorApplication orderGeneratorApplication)
        {
            _orderGeneratorApplication = orderGeneratorApplication;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Order.Price % 0.01m != 0)
                ModelState.AddModelError("Order.Price", "Preço deve ser múltiplo de 0.01");

            if (!ModelState.IsValid)
            {
                Message = "Dados incorretos";
                return Page();
            }

            if(!AcceptedSymbols.Contains(Order.Symbol))
            {
                Message = "Símbolo inexistente";
                return Page();
            }

            var clOrdId = Guid.NewGuid().ToString("N");
            char side = Order.Side == 1 ? Side.BUY : Side.SELL;

            try
            {
                var report = await _orderGeneratorApplication.SendOrderAsync(new NewOrderSingle
                {
                    ClOrdID = new ClOrdID(clOrdId),
                    Symbol = new Symbol(Order.Symbol),
                    Side = new Side(side),
                    TransactTime = new TransactTime(DateTime.UtcNow),
                    OrderQty = new OrderQty(Order.Quantity),
                    Price = new Price(Order.Price),
                    OrdType = new OrdType(OrdType.LIMIT)
                }, TimeSpan.FromSeconds(5));

                Accepted = report.ExecType.Value == ExecType.NEW;
                Message = Accepted.Value
                    ? $"Ordem {report.ClOrdID.Value} ACEITA (ExecType=New, OrderID={report.OrderID.Value})"
                    : $"Ordem {report.ClOrdID.Value} REJEITADA (ExecType=Rejected): " +
                      (report.IsSetText() ? report.Text.Value : "sem motivo");
            }
            catch (InvalidOperationException ex)
            {
                Message = ex.Message;
            }
            catch (TimeoutException)
            {
                Message = "Sem resposta do OrderAccumulator.";
            }

            return Page();
        }
    }

    public class OrderInput
    {
        [Required]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        [Range(1, 2)]
        public int Side { get; set; }

        [Required]
        [Range(1, 99_999)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 999.99)]
        public decimal Price { get; set; }
    }
}
