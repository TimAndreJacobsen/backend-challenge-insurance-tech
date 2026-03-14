using Claims.DTOs;
using Claims.Models;
using Claims.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimsService _claimsService;
    private readonly IValidator<CreateClaimRequest> _validator;

    public ClaimsController(IClaimsService claimsService, IValidator<CreateClaimRequest> validator)
    {
        _claimsService = claimsService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClaimResponse>>> GetAsync(CancellationToken cancellationToken)
    {
        var claims = await _claimsService.GetAllAsync(cancellationToken);
        return Ok(claims.Select(ClaimResponse.FromEntity));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClaimResponse>> GetAsync(string id, CancellationToken cancellationToken)
    {
        var claim = await _claimsService.GetByIdAsync(id, cancellationToken);
        if (claim is null)
            return NotFound();
        return Ok(ClaimResponse.FromEntity(claim));
    }

    [HttpPost]
    public async Task<ActionResult<ClaimResponse>> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return ValidationProblem(ModelState);
        }

        var claim = new Claim
        {
            Id = string.Empty,
            CoverId = request.CoverId,
            Created = request.Created,
            Name = request.Name,
            Type = request.Type,
            DamageCost = request.DamageCost
        };
        var created = await _claimsService.CreateAsync(claim, cancellationToken);
        return Ok(ClaimResponse.FromEntity(created));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await _claimsService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
