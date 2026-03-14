using Claims.Models;

namespace Claims.Services;

/// <summary>
/// Manage insurance covers.
/// </summary>
public interface ICoversService
{
    Task<IEnumerable<Cover>> GetAllAsync(CancellationToken ct = default);
    Task<Cover?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Cover> CreateAsync(Cover cover, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
