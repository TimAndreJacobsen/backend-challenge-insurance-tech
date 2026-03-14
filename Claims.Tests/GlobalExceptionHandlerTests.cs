using System.Net;
using System.Text.Json;
using Claims.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Claims.Tests;

public class GlobalExceptionHandlerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static (GlobalExceptionHandler handler, DefaultHttpContext context) Arrange(bool isDevelopment)
    {
        var logger = NullLogger<GlobalExceptionHandler>.Instance;
        var env = new FakeHostEnvironment(isDevelopment ? "Development" : "Production");
        var handler = new GlobalExceptionHandler(logger, env);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        return (handler, context);
    }

    private static async Task<ProblemDetails> DeserializeProblemDetails(HttpResponse response)
    {
        response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(response.Body, JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        return problem;
    }

    [Fact]
    public async Task TryHandleAsync_Returns500_WithProblemDetails()
    {
        var (handler, context) = Arrange(isDevelopment: false);
        var exception = new InvalidOperationException("Test exception");

        var handled = await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        Assert.True(handled);
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);

        var problem = await DeserializeProblemDetails(context.Response);
        Assert.Equal(500, problem.Status);
        Assert.Equal("An unexpected error occurred", problem.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.6.1", problem.Type);
    }

    [Fact]
    public async Task TryHandleAsync_InDevelopment_IncludesExceptionDetail()
    {
        var (handler, context) = Arrange(isDevelopment: true);
        var exception = new InvalidOperationException("Test exception");

        await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        var problem = await DeserializeProblemDetails(context.Response);
        Assert.Contains("Test exception", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_InProduction_DoesNotLeakStackTrace()
    {
        var (handler, context) = Arrange(isDevelopment: false);
        var exception = new InvalidOperationException("Test exception");

        await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        var problem = await DeserializeProblemDetails(context.Response);
        Assert.Null(problem.Detail);
    }

    private class FakeHostEnvironment : IHostEnvironment
    {
        public FakeHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; } // fake different env's to test when ExceptionHandler includes real exception 
        public string ApplicationName { get; set; } = "Claims.Tests";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
