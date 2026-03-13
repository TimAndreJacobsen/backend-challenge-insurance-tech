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
    public async Task<ActionResult<IEnumerable<Claim>>> GetAsync()
    {
        return Ok(await _claimsService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Claim>> GetAsync(string id)
    {
        var claim = await _claimsService.GetByIdAsync(id);
        if (claim is null)
            return NotFound();
        return Ok(claim);
    }

    [HttpPost]
    public async Task<ActionResult<Claim>> CreateAsync(Claim claim)
    {
        var created = await _claimsService.CreateAsync(claim);
        return Ok(created);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        await _claimsService.DeleteAsync(id);
        return NoContent();
    }
}
