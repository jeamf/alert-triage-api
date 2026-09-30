using AlertTriage.Domain;
using Xunit;

namespace AlertTriage.UnitTests;

public class AlertTests
{
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("", "rule", "host")]
    [InlineData("src", " ", "host")]
    [InlineData("src", "rule", "")]
    public void Create_WithBlankField_Throws(string source, string rule, string host)
    {
        Assert.Throws<ArgumentException>(() => Alert.Create(source, rule, host, Severity.High, Now));
    }

    [Fact]
    public void Create_StartsAsNewWithOneOccurrence()
    {
        var alert = Alert.Create("Wazuh", "Failed logins", "SRV-01", Severity.High, Now);

        Assert.Equal(AlertStatus.New, alert.Status);
        Assert.Equal(1, alert.OccurrenceCount);
        Assert.Equal("srv-01", alert.Host);
    }

    [Fact]
    public void Fingerprint_IgnoresHostCaseAndWhitespace()
    {
        var a = Alert.ComputeFingerprint("Wazuh", "Failed logins", "SRV-01");
        var b = Alert.ComputeFingerprint(" Wazuh ", "Failed logins", " srv-01 ");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Fingerprint_DiffersForDifferentHosts()
    {
        var a = Alert.ComputeFingerprint("Wazuh", "Failed logins", "SRV-01");
        var b = Alert.ComputeFingerprint("Wazuh", "Failed logins", "SRV-02");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void RegisterOccurrence_RaisesSeverityButNeverLowersIt()
    {
        var alert = Alert.Create("Wazuh", "Failed logins", "SRV-01", Severity.Medium, Now);

        alert.RegisterOccurrence(Severity.Critical, Now.AddMinutes(1));
        alert.RegisterOccurrence(Severity.Low, Now.AddMinutes(2));

        Assert.Equal(Severity.Critical, alert.Severity);
        Assert.Equal(3, alert.OccurrenceCount);
        Assert.Equal(Now.AddMinutes(2), alert.LastSeenAtUtc);
    }
}
