using OrderGenerator;
using OrderGenerator.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddSingleton<OrderGeneratorApplication>();
builder.Services.AddHostedService<FixInitiatorService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(
    new { Status = "API Running" }
));

app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
