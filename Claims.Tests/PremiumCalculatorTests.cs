using Claims.Models;
using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class PremiumCalculatorTests
{
    private readonly PremiumCalculator _calculator = new();

    /**
     * Testing strategy:
     * Focus on boundaries between tiers (possible edge cases)
     * Use known good values instead of sharing config with calculator (forces tests to be updated if premiums are adjusted)
     */

    [Theory]
    [InlineData(CoverType.Yacht, 1, 1_375)]
    [InlineData(CoverType.Yacht, 30, 41_250)]
    [InlineData(CoverType.Yacht, 31, 42_556.25)]
    [InlineData(CoverType.Yacht, 180, 237_187.50)]
    [InlineData(CoverType.Yacht, 181, 238_452.50)]
    [InlineData(CoverType.Yacht, 365, 471_212.50)]
    [InlineData(CoverType.PassengerShip, 1, 1_500)]
    [InlineData(CoverType.PassengerShip, 30, 45_000)]
    [InlineData(CoverType.PassengerShip, 31, 46_470)]
    [InlineData(CoverType.PassengerShip, 180, 265_500)]
    [InlineData(CoverType.PassengerShip, 181, 266_955)]
    [InlineData(CoverType.PassengerShip, 365, 534_675)]
    [InlineData(CoverType.Tanker, 1, 1_875)]
    [InlineData(CoverType.Tanker, 365, 668_343.75)]
    [InlineData(CoverType.ContainerShip, 1, 1_625)]
    [InlineData(CoverType.ContainerShip, 365, 579_231.25)]
    [InlineData(CoverType.BulkCarrier, 1, 1_625)]
    public void ComputePremium_ReturnsExpectedPremium(CoverType coverType, int days, decimal expected)
    {
        var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = _calculator.ComputePremium(startDate, startDate.AddDays(days), coverType);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputePremium_UnknownCoverType_ThrowsArgumentOutOfRange()
    {
        var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.ComputePremium(startDate, startDate.AddDays(30), (CoverType)9999));
    }

    [Fact]
    public void ComputePremium_ExceedsMaxCoverageDays_ThrowsArgumentOutOfRange()
    {
        var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.ComputePremium(startDate, startDate.AddDays(400), CoverType.Yacht));
    }

    [Fact]
    public void ComputePremium_NegativeDays_ThrowsArgumentOutOfRange()
    {
        var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.ComputePremium(startDate, startDate.AddDays(-10), CoverType.Yacht));
    }
}
