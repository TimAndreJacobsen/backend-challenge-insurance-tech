using System.Threading.Channels;
using Claims.Auditing;
using Xunit;

namespace Claims.Tests;

public class AuditerTests
{
    private static readonly DateTime FakeUtcNow = new(2026, 3, 14, 12, 0, 0, DateTimeKind.Utc);

    private static (Auditer auditer, ChannelReader<AuditMessage> reader) CreateAuditer()
    {
        var channel = Channel.CreateUnbounded<AuditMessage>();
        var timeProvider = new FakeTimeProvider(FakeUtcNow);
        var auditer = new Auditer(channel.Writer, timeProvider);
        return (auditer, channel.Reader);
    }

    [Fact]
    public void AuditClaim_EnqueuesMessage()
    {
        var (auditer, reader) = CreateAuditer();

        auditer.AuditClaim("claim-123", "POST");

        Assert.True(reader.TryRead(out var message));
        Assert.Equal("Claim", message.EntityType);
        Assert.Equal("claim-123", message.EntityId);
        Assert.Equal("POST", message.HttpRequestType);
        Assert.Equal(FakeUtcNow, message.Created);
    }

    [Fact]
    public void AuditCover_EnqueuesMessage()
    {
        var (auditer, reader) = CreateAuditer();

        auditer.AuditCover("cover-123", "DELETE");

        Assert.True(reader.TryRead(out var message));
        Assert.Equal("Cover", message.EntityType);
        Assert.Equal("cover-123", message.EntityId);
        Assert.Equal("DELETE", message.HttpRequestType);
        Assert.Equal(FakeUtcNow, message.Created);
    }

    [Fact]
    public void AuditClaim_DoesNotBlock()
    {
        var (auditer, reader) = CreateAuditer();

        auditer.AuditClaim("id-1", "POST");
        auditer.AuditClaim("id-2", "DELETE");

        Assert.True(reader.TryRead(out var first));
        Assert.Equal("id-1", first.EntityId);
        Assert.True(reader.TryRead(out var second));
        Assert.Equal("id-2", second.EntityId);
        Assert.False(reader.TryRead(out _));
    }

    private class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FakeTimeProvider(DateTime utcNow) => _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
