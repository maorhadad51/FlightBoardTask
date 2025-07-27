namespace FlightBoard.Domain;

public enum FlightStatus
{
    Scheduled = 0,
    Boarding = 1,
    Departed = 2,
    Landed = 3
}

public class Flight
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string Airline { get; set; } = "Generic";
    public string Origin { get; set; } = "TLV";
    public string Destination { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public DateTime? EstimatedTime { get; set; }
    public string Gate { get; set; } = string.Empty;
    public bool IsArrival { get; set; }
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public string? Remarks { get; set; }
}