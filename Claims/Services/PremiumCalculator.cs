using Claims.Models;

namespace Claims.Services;

/// <inheritdoc />
public class PremiumCalculator : IPremiumCalculator
{
    private const decimal BaseDayRate = 1250m;
    
    private const decimal YachtTier2Discount = 0.05m;
    private const decimal YachtTier3Discount = 0.03m;
    private const decimal DefaultTier2Discount = 0.02m;
    private const decimal DefaultTier3Discount = 0.01m;

    private const int Tier1MaxDays = 30;
    private const int Tier2MaxDays = 150;
    private const int Tier3MaxDays = 185;
    private const int MaxCoverageDays = Tier1MaxDays + Tier2MaxDays + Tier3MaxDays;

    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        var totalDays = (endDate.Date - startDate.Date).Days;

        if (totalDays > MaxCoverageDays || totalDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(endDate), $"Invalid coverage period: {totalDays} days. Must be between 1 and {MaxCoverageDays} days.");

        var multiplier = coverType switch
        {
            CoverType.Yacht => 1.1m,
            CoverType.PassengerShip => 1.2m,
            CoverType.Tanker => 1.5m,
            CoverType.ContainerShip => 1.3m,
            CoverType.BulkCarrier => 1.3m,
            _ => throw new ArgumentOutOfRangeException(nameof(coverType), $"Unexpected cover type value: {coverType}")
        };

        var (tier2Discount, tier3Discount) = coverType switch
        {
            CoverType.Yacht => (YachtTier2Discount, YachtTier3Discount),
            _ => (DefaultTier2Discount, DefaultTier3Discount)
        };

        var premiumPerDay = BaseDayRate * multiplier;

        // Clamp min and max days for each tier
        var tier1Days = Math.Min(totalDays, Tier1MaxDays);
        var tier2Days = Math.Min(Math.Max(totalDays - Tier1MaxDays, 0), Tier2MaxDays);
        var tier3Days = Math.Min(Math.Max(totalDays - (Tier1MaxDays + Tier2MaxDays), 0), Tier3MaxDays);
        
        var totalPremium = premiumPerDay * tier1Days 
            + premiumPerDay * (1 - tier2Discount) * tier2Days
            + premiumPerDay * (1 - tier2Discount - tier3Discount) * tier3Days;

        return totalPremium;
    }
}
