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
    public DateTime CreatedAtUtc { get; private set; }

    private Alert() { } // exigido pelo EF Core

    public static Alert Create(string source, string rule, string host, Severity severity)
    {
        if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Source é obrigatório.", nameof(source));
        if (string.IsNullOrWhiteSpace(rule)) throw new ArgumentException("Rule é obrigatório.", nameof(rule));
        if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("Host é obrigatório.", nameof(host));

        return new Alert
        {
            Id = Guid.NewGuid(),
            Source = source.Trim(),
            Rule = rule.Trim(),
            Host = host.Trim().ToLowerInvariant(),
            Severity = severity,
            Status = AlertStatus.New,
            CreatedAtUtc = DateTime.UtcNow,
            Fingerprint = ComputeFingerprint(source, rule, host)
        };
    }

    private static string ComputeFingerprint(string source, string rule, string host)
    {
        var raw = $"{source.Trim()}|{rule.Trim()}|{host.Trim().ToLowerInvariant()}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }
}
