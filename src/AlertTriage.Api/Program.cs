using System.Text.Json.Serialization;
using AlertTriage.Domain;
using AlertTriage.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/alerts", async (CreateAlertRequest req, AppDbContext db) =>
{
    try
    {
        var alert = Alert.Create(req.Source, req.Rule, req.Host, req.Severity);
        db.Alerts.Add(alert);
        await db.SaveChangesAsync();
        return Results.Created($"/alerts/{alert.Id}", alert);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/alerts", async (AppDbContext db) =>
    await db.Alerts
        .AsNoTracking()
        .OrderByDescending(a => a.CreatedAtUtc)
        .Take(50)
        .ToListAsync());

app.Run();

record CreateAlertRequest(string Source, string Rule, string Host, Severity Severity);
