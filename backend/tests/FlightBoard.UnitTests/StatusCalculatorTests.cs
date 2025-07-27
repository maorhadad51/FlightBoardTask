using FlightBoard.Application;
using FlightBoard.Domain;
using FluentAssertions;

namespace FlightBoard.UnitTests;

public class StatusCalculatorTests
{
    [Theory]
    [InlineData(-61, FlightStatus.Landed)]
    [InlineData(-30, FlightStatus.Departed)]
    [InlineData(-1, FlightStatus.Departed)]
    [InlineData(0, FlightStatus.Boarding)]
    [InlineData(29, FlightStatus.Boarding)]
    [InlineData(31, FlightStatus.Scheduled)]
    public void Calculate_Returns_Expected_Status(int minutesFromNow, FlightStatus expected)
    {
        var calc = new FlightStatusCalculator();
        var now = DateTime.UtcNow;
        var scheduled = now.AddMinutes(minutesFromNow);
        calc.Calculate(scheduled, now).Should().Be(expected);
    }
}