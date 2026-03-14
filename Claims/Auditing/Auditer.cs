using System.Threading.Channels;

namespace Claims.Auditing
{
    /// <summary>
    /// Auditer that writes messages to a background channel.
    /// </summary>
    public class Auditer : IAuditer
    {
        private readonly ChannelWriter<AuditMessage> _channel;
        private readonly TimeProvider _timeProvider;

        public Auditer(ChannelWriter<AuditMessage> channel, TimeProvider timeProvider)
        {
            _channel = channel;
            _timeProvider = timeProvider;
        }

        public void AuditClaim(string id, string httpRequestType)
        {
            var message = new AuditMessage("Claim", id, httpRequestType, _timeProvider.GetUtcNow().UtcDateTime);
            _channel.TryWrite(message);
        }

        public void AuditCover(string id, string httpRequestType)
        {
            var message = new AuditMessage("Cover", id, httpRequestType, _timeProvider.GetUtcNow().UtcDateTime);
            _channel.TryWrite(message);
        }
    }
}
