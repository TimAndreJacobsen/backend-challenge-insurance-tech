using System.Threading.Channels;
using Claims.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Claims.Tests;

public class AuditBackgroundServiceTests
{
    private static AuditContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AuditContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AuditContext(options);
    }

    private static (AuditBackgroundService service, ChannelWriter<AuditMessage> writer, AuditContext context) Arrange()
    {
        var channel = Channel.CreateUnbounded<AuditMessage>();
        var context = CreateInMemoryContext();

        var services = new ServiceCollection();
        services.AddSingleton(context);
        services.AddDbContext<AuditContext>(opt => opt.UseInMemoryDatabase(context.Database.GetConnectionString()!), ServiceLifetime.Singleton);

        var scopeFactory = new FakeScopeFactory(context);

        var service = new AuditBackgroundService(
            channel.Reader,
            scopeFactory,
            NullLogger<AuditBackgroundService>.Instance);

        return (service, channel.Writer, context);
    }

    [Fact]
    public async Task ProcessesClaimAuditMessage()
    {
        var (service, writer, context) = Arrange();

        writer.TryWrite(new AuditMessage("Claim", "claim-1", "POST", new DateTime(2026, 3, 14, 0, 0, 0, DateTimeKind.Utc)));
        writer.Complete();

        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!;

        var audit = Assert.Single(context.ClaimAudits);
        Assert.Equal("claim-1", audit.ClaimId);
        Assert.Equal("POST", audit.HttpRequestType);
    }

    [Fact]
    public async Task ProcessesCoverAuditMessage()
    {
        var (service, writer, context) = Arrange();

        writer.TryWrite(new AuditMessage("Cover", "cover-1", "DELETE", new DateTime(2026, 3, 14, 0, 0, 0, DateTimeKind.Utc)));
        writer.Complete();

        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!;

        var audit = Assert.Single(context.CoverAudits);
        Assert.Equal("cover-1", audit.CoverId);
        Assert.Equal("DELETE", audit.HttpRequestType);
    }

    [Fact]
    public async Task ProcessesMultipleMessages()
    {
        var (service, writer, context) = Arrange();

        writer.TryWrite(new AuditMessage("Claim", "c1", "POST", new DateTime(2026, 3, 14, 0, 0, 0, DateTimeKind.Utc)));
        writer.TryWrite(new AuditMessage("Cover", "v1", "DELETE", new DateTime(2026, 3, 14, 0, 0, 0, DateTimeKind.Utc)));
        writer.TryWrite(new AuditMessage("Claim", "c2", "DELETE", new DateTime(2026, 3, 14, 0, 0, 0, DateTimeKind.Utc)));
        writer.Complete();

        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!;

        Assert.Equal(2, context.ClaimAudits.Count());
        Assert.Single(context.CoverAudits);
    }

    private class FakeScopeFactory : IServiceScopeFactory
    {
        private readonly AuditContext _context;
        public FakeScopeFactory(AuditContext context) => _context = context;

        public IServiceScope CreateScope() => new FakeScope(_context);

        private class FakeScope : IServiceScope
        {
            public FakeScope(AuditContext context)
            {
                ServiceProvider = new FakeServiceProvider(context);
            }

            public IServiceProvider ServiceProvider { get; }
            public void Dispose() { }
        }

        private class FakeServiceProvider : IServiceProvider
        {
            private readonly AuditContext _context;
            public FakeServiceProvider(AuditContext context) => _context = context;

            public object? GetService(Type serviceType) =>
                serviceType == typeof(AuditContext) ? _context : null;
        }
    }
}
