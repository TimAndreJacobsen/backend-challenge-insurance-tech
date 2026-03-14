using Claims.DTOs;
using Claims.Models;
using Claims.Validators;
using Xunit;

namespace Claims.Tests;

public class CreateCoverRequestValidatorTests
{
    private static readonly DateTime Today = new(2026, 3, 14, 0, 0, 0, DateTimeKind.Utc);

    private static CreateCoverRequestValidator CreateValidator(DateTime? today = null)
    {
        var timeProvider = new FakeTimeProvider(today ?? Today);
        return new CreateCoverRequestValidator(timeProvider);
    }

    private static CreateCoverRequest ValidRequest() => new(
        StartDate: Today,
        EndDate: Today.AddDays(180),
        Type: CoverType.Yacht
    );

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var validator = CreateValidator();
        var result = await validator.ValidateAsync(ValidRequest(), TestContext.Current.CancellationToken);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task StartDate_InThePast_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { StartDate = Today.AddDays(-1) };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "StartDate");
        Assert.Equal("StartDate cannot be in the past.", error.ErrorMessage);
    }

    [Fact]
    public async Task StartDate_Today_Passes()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { StartDate = Today };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task StartDate_InTheFuture_Passes()
    {
        var validator = CreateValidator();
        var futureStart = Today.AddDays(30);
        var request = ValidRequest() with { StartDate = futureStart, EndDate = futureStart.AddDays(180) };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task EndDate_BeforeStartDate_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { EndDate = Today.AddDays(-1) };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "EndDate");
        Assert.Equal("EndDate must be after StartDate.", error.ErrorMessage);
    }

    [Fact]
    public async Task EndDate_SameAsStartDate_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { EndDate = Today };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "EndDate");
        Assert.Equal("EndDate must be after StartDate.", error.ErrorMessage);
    }

    [Fact]
    public async Task Period_Exactly365Days_Passes()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { EndDate = Today.AddDays(365) };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Period_Exceeds365Days_Fails()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { EndDate = Today.AddDays(366) };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "EndDate");
        Assert.Equal("Total insurance period cannot exceed 1 year.", error.ErrorMessage);
    }

    [Fact]
    public async Task Period_1Day_Passes()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { EndDate = Today.AddDays(1) };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task MultipleErrors_AllReported()
    {
        var validator = CreateValidator();
        var request = ValidRequest() with { StartDate = Today.AddDays(-1), EndDate = Today.AddDays(-2) };
        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartDate");
        Assert.Contains(result.Errors, e => e.PropertyName == "EndDate");
        Assert.Equal(2, result.Errors.Count);
    }

    private class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FakeTimeProvider(DateTime utcNow) => _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
