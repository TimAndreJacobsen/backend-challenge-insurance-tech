using Claims.DTOs;
using Claims.Services;
using FluentValidation;

namespace Claims.Validators;

public class CreateClaimRequestValidator : AbstractValidator<CreateClaimRequest>
{
    public CreateClaimRequestValidator(ICoversService coversService)
    {
        RuleFor(x => x.DamageCost)
            .GreaterThan(0)
            .WithMessage("DamageCost must be greater than 0.")
            .LessThanOrEqualTo(100_000)
            .WithMessage("DamageCost cannot exceed 100,000.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.");

        RuleFor(x => x.CoverId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("CoverId is required.")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("CoverId must be a valid GUID.");

        RuleFor(x => x)
            .CustomAsync(async (request, context, ct) =>
            {
                var cover = await coversService.GetByIdAsync(request.CoverId, ct);
                if (cover is null)
                {
                    context.AddFailure("CoverId", "Cover not found.");
                    return;
                }

                if (request.Created < cover.StartDate || request.Created > cover.EndDate)
                    context.AddFailure("Created", "Created date must be within the period of the related Cover.");
            })
            .When(x => Guid.TryParse(x.CoverId, out _));
    }
}
