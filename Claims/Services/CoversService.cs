using Claims.Auditing;
using Claims.Models;
using Claims.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Claims.Services;

/// <inheritdoc />
public class CoversService : ICoversService
{
    private readonly ClaimsContext _context;
    private readonly IAuditer _auditer;
    private readonly IPremiumCalculator _premiumCalculator;

    public CoversService(ClaimsContext context, IAuditer auditer, IPremiumCalculator premiumCalculator)
    {
        _context = context;
        _auditer = auditer;
        _premiumCalculator = premiumCalculator;
    }

    public async Task<IEnumerable<Cover>> GetAllAsync()
    {
        return await _context.Covers.ToListAsync();
    }

    public async Task<Cover?> GetByIdAsync(string id)
    {
        return await _context.Covers
            .Where(c => c.Id == id)
            .SingleOrDefaultAsync();
    }

    public async Task<Cover> CreateAsync(Cover cover)
    {
        cover.Id = Guid.NewGuid().ToString();
        cover.Premium = _premiumCalculator.ComputePremium(cover.StartDate, cover.EndDate, cover.Type);
        _context.Covers.Add(cover);
        await _context.SaveChangesAsync();
        _auditer.AuditCover(cover.Id, "POST");
        return cover;
    }

    public async Task DeleteAsync(string id)
    {
        _auditer.AuditCover(id, "DELETE");
        var cover = await GetByIdAsync(id);
        if (cover is not null)
        {
            _context.Covers.Remove(cover);
            await _context.SaveChangesAsync();
        }
    }
}
