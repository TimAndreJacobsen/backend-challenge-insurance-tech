using Claims.DTOs;
using Claims.Models;
using Claims.Services;
using Claims.Validators;
using Xunit;

namespace Claims.Tests;

public class CreateClaimRequestValidatorTests
{

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var validator = CreateValidator();
        var result = await validator.ValidateAsync(ValidRequest(), TestContext.Current.CancellationToken);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task DamageCost_ExceedsLimit_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { DamageCost = 100_001m };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "DamageCost");
        Assert.Equal("DamageCost cannot exceed 100,000.", error.ErrorMessage);
    }

    [Fact]
    public async Task DamageCost_AtLimit_Passes()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { DamageCost = 100_000m };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task DamageCost_Zero_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { DamageCost = 0m };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "DamageCost");
        Assert.Equal("DamageCost must be greater than 0.", error.ErrorMessage);
    }

    [Fact]
    public async Task DamageCost_Negative_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { DamageCost = -1m };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "DamageCost");
        Assert.Equal("DamageCost must be greater than 0.", error.ErrorMessage);
    }

    [Fact]
    public async Task CreatedDate_BeforeCoverStart_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { Created = new DateTime(2025, 12, 31) };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Created");
        Assert.Equal("Created date must be within the period of the related Cover.", error.ErrorMessage);
    }

    [Fact]
    public async Task CreatedDate_AfterCoverEnd_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { Created = new DateTime(2027, 1, 1) };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Created");
        Assert.Equal("Created date must be within the period of the related Cover.", error.ErrorMessage);
    }

    [Fact]
    public async Task CreatedDate_OnCoverStartDate_Passes()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { Created = new DateTime(2026, 1, 1) };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreatedDate_OnCoverEndDate_Passes()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { Created = new DateTime(2026, 12, 31) };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CoverNotFound_Fails()
    {
        var coversService = new StubCoversService(cover: null);
        var validator = new CreateClaimRequestValidator(coversService);
        var result = await validator.ValidateAsync(ValidRequest(), TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal("CoverId", error.PropertyName);
        Assert.Equal("Cover not found.", error.ErrorMessage);
    }

    [Fact]
    public async Task CoverId_DoesNotMatchAnyCover_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { CoverId = Guid.NewGuid().ToString() };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal("CoverId", error.PropertyName);
        Assert.Equal("Cover not found.", error.ErrorMessage);
    }

    [Fact]
    public async Task EmptyName_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { Name = "" };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Name");
        Assert.Equal("Name is required.", error.ErrorMessage);
    }

    [Fact]
    public async Task EmptyCoverId_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { CoverId = "" };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "CoverId");
        Assert.Equal("CoverId is required.", error.ErrorMessage);
    }

    [Fact]
    public async Task CoverId_InvalidGuid_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { CoverId = "not-a-guid" };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "CoverId");
        Assert.Equal("CoverId must be a valid GUID.", error.ErrorMessage);
    }

    [Fact]
    public async Task MultipleErrors_AllReported()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { Name = "", DamageCost = 0m };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        Assert.Contains(result.Errors, e => e.PropertyName == "DamageCost");
        Assert.True(result.Errors.Count >= 2);
    }

    private static readonly string ValidCoverId = Guid.NewGuid().ToString();

    private static readonly Cover ValidCover = new()
    {
        Id = ValidCoverId,
        StartDate = new DateTime(2026, 1, 1),
        EndDate = new DateTime(2026, 12, 31),
        Type = CoverType.Yacht,
        Premium = 10000m
    };

    private static CreateClaimRequestValidator CreateValidator(Cover? cover = null)
    {
        var coversService = new StubCoversService(cover ?? ValidCover);
        return new CreateClaimRequestValidator(coversService);
    }

    private static CreateClaimRequest ValidRequest() => new(
        CoverId: ValidCoverId,
        Created: new DateTime(2026, 6, 15),
        Name: "Storm damage",
        Type: ClaimType.BadWeather,
        DamageCost: 50_000m
    );

    private class StubCoversService : ICoversService
    {
        private readonly Cover? _cover;

        public StubCoversService(Cover? cover) => _cover = cover;

        public Task<Cover?> GetByIdAsync(string id, CancellationToken ct = default)
            => Task.FromResult(_cover?.Id == id ? _cover : null);

        public Task<IEnumerable<Cover>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IEnumerable<Cover>>(Array.Empty<Cover>());

        public Task<Cover> CreateAsync(Cover cover, CancellationToken ct = default)
            => Task.FromResult(cover);

        public Task DeleteAsync(string id, CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
