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

    public async Task<IEnumerable<Cover>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Covers.ToListAsync(ct);
    }

    public async Task<Cover?> GetByIdAsync(string id, CancellationToken ct)
    {
        return await _context.Covers
            .Where(c => c.Id == id)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<Cover> CreateAsync(Cover cover, CancellationToken ct)
    {
        cover.Id = Guid.NewGuid().ToString();
        cover.Premium = _premiumCalculator.ComputePremium(cover.StartDate, cover.EndDate, cover.Type);
        _context.Covers.Add(cover);
        await _context.SaveChangesAsync(ct);
        _auditer.AuditCover(cover.Id, "POST");
        return cover;
    }

    public async Task DeleteAsync(string id, CancellationToken ct)
    {
        _auditer.AuditCover(id, "DELETE");
        var cover = await GetByIdAsync(id, ct);
        if (cover is not null)
        {
            _context.Covers.Remove(cover);
            await _context.SaveChangesAsync(ct);
        }
    }
}
