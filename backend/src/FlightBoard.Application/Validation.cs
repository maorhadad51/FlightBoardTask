using FluentValidation;

namespace FlightBoard.Application;

public class CreateFlightRequestValidator : AbstractValidator<CreateFlightRequest>
{
    public CreateFlightRequestValidator()
    {
        RuleFor(x => x.FlightNumber).NotEmpty();
        RuleFor(x => x.Destination).NotEmpty();
        RuleFor(x => x.Gate).NotEmpty();
        RuleFor(x => x.ScheduledTime).Must(dt => dt.ToUniversalTime() > DateTime.UtcNow).WithMessage("ScheduledTime must be in the future");
    }
}