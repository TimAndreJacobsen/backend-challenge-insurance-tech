using Claims.Models;

namespace Claims.Services;

/// <summary>
/// Service for managing insurance claims.
/// </summary>
public interface IClaimsService
{
    Task<IEnumerable<Claim>> GetAllAsync(CancellationToken ct = default);
    Task<Claim?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Claim> CreateAsync(Claim claim, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
