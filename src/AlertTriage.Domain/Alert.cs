using System.Security.Cryptography;
using System.Text;

namespace AlertTriage.Domain;

public enum Severity { Low = 1, Medium = 2, High = 3, Critical = 4 }

public enum AlertStatus { New, InTriage, FalsePositive, Escalated, Closed }

public class Alert
{
    public Guid Id { get; private set; }
    public string Source { get; private set; } = default!;
    public string Rule { get; private set; } = default!;
    public string Host { get; private set; } = default!;
    public Severity Severity { get; private set; }
    public AlertStatus Status { get; private set; }
    public string Fingerprint { get; private set; } = default!;
    public int OccurrenceCount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastSeenAtUtc { get; private set; }

    private Alert() { } // exigido pelo EF Core

    public static Alert Create(string source, string rule, string host, Severity severity, DateTime nowUtc)
    {
        var fingerprint = ComputeFingerprint(source, rule, host);

        return new Alert
        {
            Id = Guid.NewGuid(),
            Source = source.Trim(),
            Rule = rule.Trim(),
            Host = host.Trim().ToLowerInvariant(),
            Severity = severity,
            Status = AlertStatus.New,
            Fingerprint = fingerprint,
            OccurrenceCount = 1,
            CreatedAtUtc = nowUtc,
            LastSeenAtUtc = nowUtc
        };
    }

    public void RegisterOccurrence(Severity severity, DateTime nowUtc)
    {
        OccurrenceCount++;
        LastSeenAtUtc = nowUtc;
        if (severity > Severity) Severity = severity; // nunca reduz a severidade
    }

    public static string ComputeFingerprint(string source, string rule, string host)
    {
        if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Source é obrigatório.", nameof(source));
        if (string.IsNullOrWhiteSpace(rule)) throw new ArgumentException("Rule é obrigatório.", nameof(rule));
        if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("Host é obrigatório.", nameof(host));

        var raw = $"{source.Trim()}|{rule.Trim()}|{host.Trim().ToLowerInvariant()}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }
}
