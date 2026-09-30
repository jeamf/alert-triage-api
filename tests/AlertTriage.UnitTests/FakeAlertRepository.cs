using AlertTriage.Application;
using AlertTriage.Domain;

namespace AlertTriage.UnitTests;

public class FakeAlertRepository : IAlertRepository
{
    public List<Alert> Store { get; } = new();

    public Task<Alert?> FindOpenByFingerprintAsync(string fingerprint, DateTime seenSinceUtc, CancellationToken ct) =>
        Task.FromResult(Store
            .Where(a => a.Fingerprint == fingerprint
                        && a.Status != AlertStatus.Closed
                        && a.LastSeenAtUtc >= seenSinceUtc)
            .OrderByDescending(a => a.LastSeenAtUtc)
            .FirstOrDefault());

    public Task AddAsync(Alert alert, CancellationToken ct)
    {
        Store.Add(alert);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Alert>> ListRecentAsync(int take, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Alert>>(
            Store.OrderByDescending(a => a.LastSeenAtUtc).Take(take).ToList());

    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}
