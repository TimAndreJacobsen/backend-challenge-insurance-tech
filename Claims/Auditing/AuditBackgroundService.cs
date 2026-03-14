using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;

namespace Claims.Auditing;

/// <summary>
/// Background service that reads audit messages from a channel and persists them to database.
/// </summary>
public class AuditBackgroundService : BackgroundService
{
    private readonly ChannelReader<AuditMessage> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditBackgroundService> _logger;

    public AuditBackgroundService(
        ChannelReader<AuditMessage> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<AuditBackgroundService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // This is a sequential, in-memory implementation that has several major limitations that need to be addressed for production use.
    // TODO: 
    // 1. Use Azure Service Bus to avoid data loss on shutdown and enable scaling with multiple instances of the api.
    // 2. Implement retry with Polly
    // 3. Implement dead-letter queue
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AuditContext>();

                switch (message.EntityType)
                {
                    case "Claim":
                        context.ClaimAudits.Add(new ClaimAudit
                        {
                            ClaimId = message.EntityId,
                            Created = message.Created,
                            HttpRequestType = message.HttpRequestType
                        });
                        break;
                    case "Cover":
                        context.CoverAudits.Add(new CoverAudit
                        {
                            CoverId = message.EntityId,
                            Created = message.Created,
                            HttpRequestType = message.HttpRequestType
                        });
                        break;
                }

                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to persist audit message. EntityType={EntityType}, EntityId={EntityId}, HttpRequestType={HttpRequestType}, Created={Created}",
                    message.EntityType, message.EntityId, message.HttpRequestType, message.Created);
            }
        }
    }
}
