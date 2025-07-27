using FlightBoard.Domain;

namespace FlightBoard.Application;

public interface IFlightStatusCalculator
{
    FlightStatus Calculate(DateTime scheduledUtc, DateTime nowUtc);
}

public class FlightStatusCalculator : IFlightStatusCalculator
{
    public FlightStatus Calculate(DateTime scheduledUtc, DateTime nowUtc)
    {
        var delta = nowUtc - scheduledUtc;
        if (nowUtc < scheduledUtc.AddMinutes(-30)) return FlightStatus.Scheduled;
        if (nowUtc >= scheduledUtc.AddMinutes(-30) && nowUtc < scheduledUtc) return FlightStatus.Boarding;
        if (nowUtc >= scheduledUtc && nowUtc <= scheduledUtc.AddMinutes(60)) return FlightStatus.Departed;
        return FlightStatus.Landed;
    }
}