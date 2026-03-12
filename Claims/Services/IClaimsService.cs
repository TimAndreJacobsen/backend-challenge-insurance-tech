namespace Claims.Services;

/// <summary>
/// Service for managing insurance claims.
/// </summary>
public interface IClaimsService
{
    Task<IEnumerable<Claim>> GetAllAsync();
    Task<Claim?> GetByIdAsync(string id);
    Task<Claim> CreateAsync(Claim claim);
    Task DeleteAsync(string id);
}
