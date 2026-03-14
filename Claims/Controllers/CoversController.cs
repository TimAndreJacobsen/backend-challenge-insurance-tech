using Claims.DTOs;
using Claims.Models;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ICoversService _coversService;
    private readonly IPremiumCalculator _premiumCalculator;

    public CoversController(ICoversService coversService, IPremiumCalculator premiumCalculator)
    {
        _coversService = coversService;
        _premiumCalculator = premiumCalculator;
    }

    [HttpPost("compute")]
    public ActionResult<decimal> ComputePremiumAsync(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        return Ok(_premiumCalculator.ComputePremium(startDate, endDate, coverType));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoverResponse>>> GetAsync(CancellationToken cancellationToken)
    {
        var covers = await _coversService.GetAllAsync(cancellationToken);
        return Ok(covers.Select(CoverResponse.FromEntity));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CoverResponse>> GetAsync(string id, CancellationToken cancellationToken)
    {
        var cover = await _coversService.GetByIdAsync(id, cancellationToken);
        if (cover is null)
            return NotFound();
        return Ok(CoverResponse.FromEntity(cover));
    }

    [HttpPost]
    public async Task<ActionResult<CoverResponse>> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken)
    {
        var cover = new Cover
        {
            Id = string.Empty,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Type = request.Type
        };
        var created = await _coversService.CreateAsync(cover, cancellationToken);
        return Ok(CoverResponse.FromEntity(created));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await _coversService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
