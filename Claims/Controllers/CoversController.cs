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
    public async Task<ActionResult<IEnumerable<Cover>>> GetAsync()
    {
        return Ok(await _coversService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Cover>> GetAsync(string id)
    {
        var cover = await _coversService.GetByIdAsync(id);
        if (cover is null)
            return NotFound();
        return Ok(cover);
    }

    [HttpPost]
    public async Task<ActionResult<Cover>> CreateAsync(Cover cover)
    {
        var created = await _coversService.CreateAsync(cover);
        return Ok(created);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        await _coversService.DeleteAsync(id);
        return NoContent();
    }
}
