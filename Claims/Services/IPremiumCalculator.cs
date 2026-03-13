using Claims.Models;

namespace Claims.Services;

/// <summary>
/// Computes insurance premiums based on cover type and period.
/// </summary>
public interface IPremiumCalculator
{
    decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType);
}
