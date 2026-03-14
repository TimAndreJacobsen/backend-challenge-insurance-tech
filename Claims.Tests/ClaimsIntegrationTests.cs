using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.DTOs;
using Claims.Models;
using Claims.Tests.Fixtures;
using Xunit;

namespace Claims.Tests;

[Collection("ClaimsIntegration")]
public class ClaimsIntegrationTests
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public ClaimsIntegrationTests(ClaimsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/Claims", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidClaim_ReturnsOkWithClaim()
    {
        var cover = await CreateCoverAsync();
        var request = ValidClaimRequest(cover);

        var response = await _client.PostAsJsonAsync("/Claims", request, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var claim = await response.Content.ReadFromJsonAsync<ClaimResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(claim);
        Assert.NotEmpty(claim.Id);
        Assert.Equal(cover.Id, claim.CoverId);
        Assert.Equal(request.Name, claim.Name);
        Assert.Equal(request.Type, claim.Type);
        Assert.Equal(request.Created, claim.Created);
        Assert.Equal(request.DamageCost, claim.DamageCost);
    }

    [Fact]
    public async Task GetById_ExistingClaim_ReturnsClaim()
    {
        var cover = await CreateCoverAsync();
        var created = await CreateClaimAsync(cover);

        var response = await _client.GetAsync($"/Claims/{created.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var claim = await response.Content.ReadFromJsonAsync<ClaimResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(claim);
        Assert.Equal(created.Id, claim.Id);
        Assert.Equal(created.CoverId, claim.CoverId);
        Assert.Equal(created.Name, claim.Name);
        Assert.Equal(created.Type, claim.Type);
        Assert.Equal(created.Created, claim.Created);
        Assert.Equal(created.DamageCost, claim.DamageCost);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/Claims/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingClaim_ReturnsNoContent()
    {
        var cover = await CreateCoverAsync();
        var created = await CreateClaimAsync(cover);

        var response = await _client.DeleteAsync($"/Claims/{created.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ThenGet_ReturnsNotFound()
    {
        var cover = await CreateCoverAsync();
        var created = await CreateClaimAsync(cover);

        await _client.DeleteAsync($"/Claims/{created.Id}", TestContext.Current.CancellationToken);
        var response = await _client.GetAsync($"/Claims/{created.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_NonExistentCoverId_ReturnsUnprocessableEntity()
    {
        var request = new CreateClaimRequest(
            CoverId: Guid.NewGuid().ToString(),
            Created: DateTime.UtcNow.Date.AddDays(1),
            Name: "Test Claim",
            Type: ClaimType.Collision,
            DamageCost: 1000m
        );

        var response = await _client.PostAsJsonAsync("/Claims", request, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyName_ReturnsUnprocessableEntity()
    {
        var cover = await CreateCoverAsync();
        var request = new CreateClaimRequest(
            CoverId: cover.Id,
            Created: cover.StartDate.AddDays(1),
            Name: "",
            Type: ClaimType.Collision,
            DamageCost: 1000m
        );

        var response = await _client.PostAsJsonAsync("/Claims", request, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_DamageCostExceedsLimit_ReturnsUnprocessableEntity()
    {
        var cover = await CreateCoverAsync();
        var request = new CreateClaimRequest(
            CoverId: cover.Id,
            Created: cover.StartDate.AddDays(1),
            Name: "Expensive Claim",
            Type: ClaimType.Fire,
            DamageCost: 100_001m
        );

        var response = await _client.PostAsJsonAsync("/Claims", request, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_DamageCostZero_ReturnsUnprocessableEntity()
    {
        var cover = await CreateCoverAsync();
        var request = new CreateClaimRequest(
            CoverId: cover.Id,
            Created: cover.StartDate.AddDays(1),
            Name: "Zero Damage",
            Type: ClaimType.Collision,
            DamageCost: 0m
        );

        var response = await _client.PostAsJsonAsync("/Claims", request, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_CreatedDateOutsideCoverPeriod_ReturnsUnprocessableEntity()
    {
        var cover = await CreateCoverAsync();
        var request = new CreateClaimRequest(
            CoverId: cover.Id,
            Created: cover.EndDate.AddDays(1),
            Name: "Late Claim",
            Type: ClaimType.Fire,
            DamageCost: 5000m
        );

        var response = await _client.PostAsJsonAsync("/Claims", request, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    private async Task<CoverResponse> CreateCoverAsync()
    {
        var request = new CreateCoverRequest(
            StartDate: DateTime.UtcNow.Date.AddDays(1),
            EndDate: DateTime.UtcNow.Date.AddDays(31),
            Type: CoverType.Yacht
        );
        var response = await _client.PostAsJsonAsync("/Covers", request, JsonOptions, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CoverResponse>(JsonOptions, TestContext.Current.CancellationToken))!;
    }

    private async Task<ClaimResponse> CreateClaimAsync(CoverResponse cover)
    {
        var request = ValidClaimRequest(cover);
        var response = await _client.PostAsJsonAsync("/Claims", request, JsonOptions, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ClaimResponse>(JsonOptions, TestContext.Current.CancellationToken))!;
    }

    private static CreateClaimRequest ValidClaimRequest(CoverResponse cover) => new(
        CoverId: cover.Id,
        Created: cover.StartDate.AddDays(1),
        Name: "Test Claim",
        Type: ClaimType.Collision,
        DamageCost: 5000m
    );
}
