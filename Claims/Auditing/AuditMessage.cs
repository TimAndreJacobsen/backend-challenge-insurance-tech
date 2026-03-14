namespace Claims.Auditing;

public record AuditMessage(string EntityType, string EntityId, string HttpRequestType, DateTime Created);
