using FluentValidation.AspNetCore;
using FluentValidation;
using System.Text.Json.Serialization;
using FlightBoard.Application;
using FlightBoard.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// DB (SQLite)
var cs = builder.Configuration.GetConnectionString("Sqlite") ?? builder.Configuration["ConnectionStrings:Sqlite"] ?? "Data Source=flightboard.db";
builder.Services.AddDbContext<FlightDbContext>(opt => opt.UseSqlite(cs));

// MVC + JSON
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<CreateFlightRequest>, CreateFlightRequestValidator>();

// SignalR
builder.Services.AddSignalR();

// App Services
builder.Services.AddSingleton<IFlightStatusCalculator, FlightStatusCalculator>();

// Swagger (Development only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo{ Title = "FlightBoard API", Version = "v1"}));

// CORS (Dev only)
builder.Services.AddCors(o => o.AddPolicy("dev", p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();

// Ensure DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
    db.Database.EnsureCreated();
    if (!db.Flights.Any())
    {
        db.Flights.AddRange(new []
        {
            new FlightBoard.Domain.Flight{ FlightNumber="FB1001", Destination="LHR", Gate="A1", ScheduledTime=DateTime.UtcNow.AddHours(2) },
            new FlightBoard.Domain.Flight{ FlightNumber="FB1002", Destination="AMS", Gate="B2", ScheduledTime=DateTime.UtcNow.AddMinutes(20) },
            new FlightBoard.Domain.Flight{ FlightNumber="FB1003", Destination="JFK", Gate="C3", ScheduledTime=DateTime.UtcNow.AddHours(-1) },
        });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("dev");
}

app.MapGet("/health", () => Results.Ok("OK"));

app.MapHub<FlightsHub>("/hubs/flights");

app.MapControllers();

app.Run();

// Hubs
public class FlightsHub : Microsoft.AspNetCore.SignalR.Hub { }