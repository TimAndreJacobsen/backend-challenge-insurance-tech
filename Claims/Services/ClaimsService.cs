using Claims.Auditing;
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

    public async Task<IEnumerable<Claim>> GetAllAsync()
    {
        return await _context.Claims.ToListAsync();
    }

    public async Task<Claim?> GetByIdAsync(string id)
    {
        return await _context.Claims
            .Where(c => c.Id == id)
            .SingleOrDefaultAsync();
    }

    public async Task<Claim> CreateAsync(Claim claim)
    {
        claim.Id = Guid.NewGuid().ToString();
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();
        _auditer.AuditClaim(claim.Id, "POST");
        return claim;
    }

    public async Task DeleteAsync(string id)
    {
        _auditer.AuditClaim(id, "DELETE");
        var claim = await GetByIdAsync(id);
        if (claim is not null)
        {
            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();
        }
    }
}
