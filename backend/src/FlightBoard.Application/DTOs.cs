namespace FlightBoard.Application;

public record FlightDto(
    int Id,
    string FlightNumber,
    string Airline,
    string Origin,
    string Destination,
    DateTime ScheduledTime,
    DateTime? EstimatedTime,
    string Gate,
    bool IsArrival,
    DateTime LastUpdatedAt,
    string? Remarks,
    string Status
);

public record CreateFlightRequest(
    string FlightNumber,
    string Destination,
    string Gate,
    DateTime ScheduledTime,
    string Airline = "Generic",
    string Origin = "TLV",
    bool IsArrival = false,
    DateTime? EstimatedTime = null,
    string? Remarks = null
);