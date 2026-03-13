using Claims.DTOs;
using Claims.Models;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimsService _claimsService;

    public ClaimsController(IClaimsService claimsService)
    {
        _claimsService = claimsService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClaimResponse>>> GetAsync()
    {
        var claims = await _claimsService.GetAllAsync();
        return Ok(claims.Select(ClaimResponse.FromEntity));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClaimResponse>> GetAsync(string id)
    {
        var claim = await _claimsService.GetByIdAsync(id);
        if (claim is null)
            return NotFound();
        return Ok(ClaimResponse.FromEntity(claim));
    }

    [HttpPost]
    public async Task<ActionResult<ClaimResponse>> CreateAsync(CreateClaimRequest request)
    {
        var claim = new Claim
        {
            Id = string.Empty,
            CoverId = request.CoverId,
            Created = request.Created,
            Name = request.Name,
            Type = request.Type,
            DamageCost = request.DamageCost
        };
        var created = await _claimsService.CreateAsync(claim);
        return Ok(ClaimResponse.FromEntity(created));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        await _claimsService.DeleteAsync(id);
        return NoContent();
    }
}
