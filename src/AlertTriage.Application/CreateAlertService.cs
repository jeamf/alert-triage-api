using AlertTriage.Domain;

namespace AlertTriage.Application;

public record CreateAlertCommand(string Source, string Rule, string Host, Severity Severity);

public record CreateAlertResult(Alert Alert, bool IsDuplicate);

public class CreateAlertService(IAlertRepository repository, TimeProvider clock)
{
    public static readonly TimeSpan DeduplicationWindow = TimeSpan.FromMinutes(15);

    public async Task<CreateAlertResult> ExecuteAsync(CreateAlertCommand command, CancellationToken ct = default)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var fingerprint = Alert.ComputeFingerprint(command.Source, command.Rule, command.Host);

        var existing = await repository.FindOpenByFingerprintAsync(fingerprint, now - DeduplicationWindow, ct);
        if (existing is not null)
        {
            existing.RegisterOccurrence(command.Severity, now);
            await repository.SaveChangesAsync(ct);
            return new CreateAlertResult(existing, IsDuplicate: true);
        }

        var alert = Alert.Create(command.Source, command.Rule, command.Host, command.Severity, now);
        await repository.AddAsync(alert, ct);
        await repository.SaveChangesAsync(ct);
        return new CreateAlertResult(alert, IsDuplicate: false);
    }
}
