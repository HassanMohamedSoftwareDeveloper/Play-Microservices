using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Play.Shared.MassTransit;
using Play.Shared.MongoDB;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMongo().AddMongoRepository<InventoryItem>("inventoryitems")
    .AddMongoRepository<CatalogItem>("catalogitems")
    .AddMassTansitWithRabbitMQ();

AddCatalogClient(builder.Services);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddCatalogClient(IServiceCollection services)
{
    Random jitterer = new();
    services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:7069");
    })
        .AddTransientHttpErrorPolicy(
        policyBuilder => policyBuilder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
            5,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromSeconds(jitterer.Next(0, 1000)),
        onRetry: (outcome, timespan, retryAttempt) =>
        {
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()
            .LogWarning("Delaying for {delayTime} seconds, then making retry {retry}", timespan.TotalSeconds, retryAttempt);
        }))
        .AddTransientHttpErrorPolicy(
          policyBuilder => policyBuilder.Or<TimeoutRejectedException>().CircuitBreakerAsync(3,
          TimeSpan.FromSeconds(15),
          onBreak: (outcome, timespan) =>
          {
              var serviceProvider = services.BuildServiceProvider();
              serviceProvider.GetService<ILogger<CatalogClient>>()
              .LogWarning("Opening the circuit for {delayTime} seconds..", timespan.TotalSeconds);
          },
          onReset: () =>
          {
              var serviceProvider = services.BuildServiceProvider();
              serviceProvider.GetService<ILogger<CatalogClient>>()
              .LogWarning("Closing the circuit..");
          }
          ))
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}
