using System.Text.Json.Serialization;
using AlertTriage.Application;
using AlertTriage.Domain;
using AlertTriage.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<CreateAlertService>();

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/alerts", async (CreateAlertRequest req, CreateAlertService service, CancellationToken ct) =>
{
    try
    {
        var result = await service.ExecuteAsync(
            new CreateAlertCommand(req.Source, req.Rule, req.Host, req.Severity), ct);

        return result.IsDuplicate
            ? Results.Ok(result.Alert)
            : Results.Created($"/alerts/{result.Alert.Id}", result.Alert);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/alerts", async (IAlertRepository repository, CancellationToken ct) =>
    await repository.ListRecentAsync(50, ct));

app.Run();

record CreateAlertRequest(string Source, string Rule, string Host, Severity Severity);
