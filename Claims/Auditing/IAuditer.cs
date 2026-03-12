namespace Claims.Auditing;

/// <summary>
/// Audit logging of Claims and Covers operations.
/// </summary>
public interface IAuditer
{
    void AuditClaim(string id, string httpRequestType);
    void AuditCover(string id, string httpRequestType);
}
