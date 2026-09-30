using AlertTriage.Domain;

namespace AlertTriage.Application;

public interface IAlertRepository
{
    Task<Alert?> FindOpenByFingerprintAsync(string fingerprint, DateTime seenSinceUtc, CancellationToken ct);
    Task AddAsync(Alert alert, CancellationToken ct);
    Task<IReadOnlyList<Alert>> ListRecentAsync(int take, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
