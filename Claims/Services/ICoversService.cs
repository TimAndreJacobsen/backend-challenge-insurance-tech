namespace Claims.Services;

/// <summary>
/// Manage insurance covers.
/// </summary>
public interface ICoversService
{
    Task<IEnumerable<Cover>> GetAllAsync();
    Task<Cover?> GetByIdAsync(string id);
    Task<Cover> CreateAsync(Cover cover);
    Task DeleteAsync(string id);
}
