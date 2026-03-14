using Claims.Auditing;
using Claims.Models;
using Claims.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Claims.Services;

/// <inheritdoc />
public class ClaimsService : IClaimsService
{
    private readonly ClaimsContext _context;
    private readonly IAuditer _auditer;

    public ClaimsService(ClaimsContext context, IAuditer auditer)
    {
        _context = context;
        _auditer = auditer;
    }

    public async Task<IEnumerable<Claim>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Claims.ToListAsync(ct);
    }

    public async Task<Claim?> GetByIdAsync(string id, CancellationToken ct)
    {
        return await _context.Claims
            .Where(c => c.Id == id)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<Claim> CreateAsync(Claim claim, CancellationToken ct)
    {
        claim.Id = Guid.NewGuid().ToString();
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync(ct);
        _auditer.AuditClaim(claim.Id, "POST");
        return claim;
    }

    public async Task DeleteAsync(string id, CancellationToken ct)
    {
        _auditer.AuditClaim(id, "DELETE");
        var claim = await GetByIdAsync(id, ct);
        if (claim is not null)
        {
            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync(ct);
        }
    }
}
