using AlertTriage.Application;
using AlertTriage.Domain;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace AlertTriage.UnitTests;

public class CreateAlertServiceTests
{
    private readonly FakeAlertRepository _repo = new();
    private readonly FakeTimeProvider _clock = new();
    private readonly CreateAlertService _service;

    public CreateAlertServiceTests()
    {
        _service = new CreateAlertService(_repo, _clock);
    }

    private static CreateAlertCommand Command(Severity severity = Severity.High, string host = "SRV-01") =>
        new("Wazuh", "Failed logins", host, severity);

    [Fact]
    public async Task FirstAlert_IsStoredAndNotDuplicate()
    {
        var result = await _service.ExecuteAsync(Command());

        Assert.False(result.IsDuplicate);
        Assert.Single(_repo.Store);
    }

    [Fact]
    public async Task SameAlertWithinWindow_IsMergedIntoExisting()
    {
        await _service.ExecuteAsync(Command(Severity.Medium));
        _clock.Advance(TimeSpan.FromMinutes(5));

        var result = await _service.ExecuteAsync(Command(Severity.Critical));

        Assert.True(result.IsDuplicate);
        Assert.Single(_repo.Store);
        Assert.Equal(2, result.Alert.OccurrenceCount);
        Assert.Equal(Severity.Critical, result.Alert.Severity);
    }

    [Fact]
    public async Task SameAlertAfterWindow_CreatesNewAlert()
    {
        await _service.ExecuteAsync(Command());
        _clock.Advance(CreateAlertService.DeduplicationWindow + TimeSpan.FromMinutes(1));

        var result = await _service.ExecuteAsync(Command());

        Assert.False(result.IsDuplicate);
        Assert.Equal(2, _repo.Store.Count);
    }

    [Fact]
    public async Task DifferentHost_IsNotDeduplicated()
    {
        await _service.ExecuteAsync(Command(host: "SRV-01"));

        var result = await _service.ExecuteAsync(Command(host: "SRV-02"));

        Assert.False(result.IsDuplicate);
        Assert.Equal(2, _repo.Store.Count);
    }

    [Fact]
    public async Task BlankHost_ThrowsAndStoresNothing()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ExecuteAsync(Command(host: "")));

        Assert.Empty(_repo.Store);
    }
}
