using AlertTriage.Application;
using AlertTriage.Domain;
using Microsoft.EntityFrameworkCore;

namespace AlertTriage.Infrastructure;

public class AlertRepository(AppDbContext db) : IAlertRepository
{
    public Task<Alert?> FindOpenByFingerprintAsync(string fingerprint, DateTime seenSinceUtc, CancellationToken ct) =>
        db.Alerts
            .Where(a => a.Fingerprint == fingerprint
                        && a.Status != AlertStatus.Closed
                        && a.LastSeenAtUtc >= seenSinceUtc)
            .OrderByDescending(a => a.LastSeenAtUtc)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(Alert alert, CancellationToken ct) =>
        await db.Alerts.AddAsync(alert, ct);

    public async Task<IReadOnlyList<Alert>> ListRecentAsync(int take, CancellationToken ct) =>
        await db.Alerts
            .AsNoTracking()
            .OrderByDescending(a => a.LastSeenAtUtc)
            .Take(take)
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
