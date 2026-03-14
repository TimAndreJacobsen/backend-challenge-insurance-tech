using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.DTOs;
using Claims.Models;
using Claims.Tests.Fixtures;
using Xunit;

namespace Claims.Tests;

[Collection("CoversIntegration")]
public class CoversIntegrationTests
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public CoversIntegrationTests(ClaimsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/Covers", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidCover_ReturnsOkWithCover()
    {
        var request = ValidRequest();

        var response = await _client.PostAsJsonAsync("/Covers", request, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cover = await response.Content.ReadFromJsonAsync<CoverResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(cover);
        Assert.NotEmpty(cover.Id);
        Assert.Equal(request.StartDate, cover.StartDate);
        Assert.Equal(request.EndDate, cover.EndDate);
        Assert.Equal(CoverType.Yacht, cover.Type);
        Assert.True(cover.Premium > 0);
    }

    [Fact]
    public async Task GetById_ExistingCover_ReturnsCover()
    {
        var created = await CreateCoverAsync(ValidRequest());

        var response = await _client.GetAsync($"/Covers/{created.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cover = await response.Content.ReadFromJsonAsync<CoverResponse>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(cover);
        Assert.Equal(created.Id, cover.Id);
        Assert.Equal(created.StartDate, cover.StartDate);
        Assert.Equal(created.EndDate, cover.EndDate);
        Assert.Equal(created.Type, cover.Type);
        Assert.Equal(created.Premium, cover.Premium);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/Covers/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingCover_ReturnsNoContent()
    {
        var created = await CreateCoverAsync(ValidRequest());

        var response = await _client.DeleteAsync($"/Covers/{created.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ThenGet_ReturnsNotFound()
    {
        var created = await CreateCoverAsync(ValidRequest());

        await _client.DeleteAsync($"/Covers/{created.Id}", TestContext.Current.CancellationToken);
        var response = await _client.GetAsync($"/Covers/{created.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_StartDateInPast_ReturnsUnprocessableEntity()
    {
        var request = new CreateCoverRequest(
            StartDate: DateTime.UtcNow.Date.AddDays(-1),
            EndDate: DateTime.UtcNow.Date.AddDays(30),
            Type: CoverType.Yacht
        );

        var response = await _client.PostAsJsonAsync("/Covers", request, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_PeriodExceeds365Days_ReturnsUnprocessableEntity()
    {
        var request = new CreateCoverRequest(
            StartDate: DateTime.UtcNow.Date.AddDays(1),
            EndDate: DateTime.UtcNow.Date.AddDays(367),
            Type: CoverType.Yacht
        );

        var response = await _client.PostAsJsonAsync("/Covers", request, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_EndDateBeforeStartDate_ReturnsUnprocessableEntity()
    {
        var request = new CreateCoverRequest(
            StartDate: DateTime.UtcNow.Date.AddDays(10),
            EndDate: DateTime.UtcNow.Date.AddDays(5),
            Type: CoverType.Yacht
        );

        var response = await _client.PostAsJsonAsync("/Covers", request, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    private static CreateCoverRequest ValidRequest() => new(
        StartDate: DateTime.UtcNow.Date.AddDays(1),
        EndDate: DateTime.UtcNow.Date.AddDays(31),
        Type: CoverType.Yacht
    );

    private async Task<CoverResponse> CreateCoverAsync(CreateCoverRequest request)
    {
        var response = await _client.PostAsJsonAsync("/Covers", request, JsonOptions, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CoverResponse>(JsonOptions, TestContext.Current.CancellationToken))!;
    }
}
