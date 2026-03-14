using Claims.DTOs;
using FluentValidation;

namespace Claims.Validators;

public class CreateCoverRequestValidator : AbstractValidator<CreateCoverRequest>
{
    public CreateCoverRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.StartDate)
            .Must(startDate => startDate.Date >= timeProvider.GetUtcNow().Date)
            .WithMessage("StartDate cannot be in the past.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("EndDate must be after StartDate.");

        RuleFor(x => x)
            .Must(x => (x.EndDate.Date - x.StartDate.Date).TotalDays <= 365)
            .WithName("EndDate")
            .WithMessage("Total insurance period cannot exceed 1 year.")
            .When(x => x.EndDate > x.StartDate);
    }
}
