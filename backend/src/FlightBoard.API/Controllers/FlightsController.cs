using FlightBoard.Application;
using FlightBoard.Domain;
using FlightBoard.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FlightBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly FlightDbContext _db;
    private readonly IFlightStatusCalculator _calc;
    private readonly IHubContext<FlightsHub> _hub;

    public FlightsController(FlightDbContext db, IFlightStatusCalculator calc, IHubContext<FlightsHub> hub)
    {
        _db = db;
        _calc = calc;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FlightDto>>> Get([FromQuery] string? status, [FromQuery] string? destination)
    {
        var q = _db.Flights.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(destination))
            q = q.Where(f => f.Destination.Contains(destination));

        var list = await q.OrderBy(f => f.ScheduledTime).ToListAsync();
        var now = DateTime.UtcNow;
        var result = list.Select(f =>
        {
            var s = _calc.Calculate(f.ScheduledTime.ToUniversalTime(), now).ToString();
            return new FlightDto(f.Id, f.FlightNumber, f.Airline, f.Origin, f.Destination, f.ScheduledTime, f.EstimatedTime, f.Gate, f.IsArrival, f.LastUpdatedAt, f.Remarks, s);
        }).ToList();

        if (!string.IsNullOrWhiteSpace(status))
            result = result.Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();

        return Ok(result);
    }

    [HttpGet("search")]
    public Task<ActionResult<IEnumerable<FlightDto>>> Search([FromQuery] string? status, [FromQuery] string? destination)
        => Get(status, destination);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FlightDto>> GetOne(int id)
    {
        var f = await _db.Flights.FindAsync(id);
        if (f == null) return NotFound();
        var now = DateTime.UtcNow;
        var s = _calc.Calculate(f.ScheduledTime.ToUniversalTime(), now).ToString();
        return new FlightDto(f.Id, f.FlightNumber, f.Airline, f.Origin, f.Destination, f.ScheduledTime, f.EstimatedTime, f.Gate, f.IsArrival, f.LastUpdatedAt, f.Remarks, s);
    }

    [HttpPost]
    public async Task<ActionResult<FlightDto>> Create([FromBody] CreateFlightRequest req)
    {
        if (await _db.Flights.AnyAsync(x => x.FlightNumber == req.FlightNumber))
            return Conflict(new { message = "FlightNumber already exists" });

        var f = new Flight
        {
            FlightNumber = req.FlightNumber,
            Destination = req.Destination,
            Gate = req.Gate,
            ScheduledTime = req.ScheduledTime.ToUniversalTime(),
            Airline = req.Airline,
            Origin = req.Origin,
            IsArrival = req.IsArrival,
            EstimatedTime = req.EstimatedTime,
            Remarks = req.Remarks,
            LastUpdatedAt = DateTime.UtcNow
        };
        _db.Flights.Add(f);
        await _db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var s = _calc.Calculate(f.ScheduledTime, now).ToString();
        var dto = new FlightDto(f.Id, f.FlightNumber, f.Airline, f.Origin, f.Destination, f.ScheduledTime, f.EstimatedTime, f.Gate, f.IsArrival, f.LastUpdatedAt, f.Remarks, s);

        await _hub.Clients.All.SendAsync("flightCreated", dto);
        return CreatedAtAction(nameof(GetOne), new { id = f.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FlightDto>> Update(int id, [FromBody] CreateFlightRequest req)
    {
        var f = await _db.Flights.FindAsync(id);
        if (f == null) return NotFound();

        if (f.FlightNumber != req.FlightNumber && await _db.Flights.AnyAsync(x => x.FlightNumber == req.FlightNumber))
            return Conflict(new { message = "FlightNumber already exists" });

        f.FlightNumber = req.FlightNumber;
        f.Destination = req.Destination;
        f.Gate = req.Gate;
        f.ScheduledTime = req.ScheduledTime.ToUniversalTime();
        f.Airline = req.Airline;
        f.Origin = req.Origin;
        f.IsArrival = req.IsArrival;
        f.EstimatedTime = req.EstimatedTime;
        f.Remarks = req.Remarks;
        f.LastUpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var s = _calc.Calculate(f.ScheduledTime, now).ToString();
        var dto = new FlightDto(f.Id, f.FlightNumber, f.Airline, f.Origin, f.Destination, f.ScheduledTime, f.EstimatedTime, f.Gate, f.IsArrival, f.LastUpdatedAt, f.Remarks, s);
        await _hub.Clients.All.SendAsync("flightUpdated", dto);
        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var f = await _db.Flights.FindAsync(id);
        if (f == null) return NotFound();
        _db.Flights.Remove(f);
        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("flightDeleted", id);
        return NoContent();
    }
}