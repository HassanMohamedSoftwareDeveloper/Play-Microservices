using Play.Catalog.Service.Entities;
using Play.Shared.MassTransit;
using Play.Shared.MongoDB;
using Play.Shared.Settings;
const string AllowedOriginSetting = "AllowedOrigin";

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
ServiceSettings serviceSettings=builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
builder.Services.AddMongo().AddMongoRepository<Item>("items")
    .AddMassTansitWithRabbitMQ();

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
    app.UseCors(corsBbuilder =>
    {
        corsBbuilder.WithOrigins(builder.Configuration[AllowedOriginSetting])
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
