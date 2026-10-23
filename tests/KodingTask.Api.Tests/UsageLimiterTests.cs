using KodingTask.Api.Limits;
using Microsoft.Extensions.Time.Testing;

namespace KodingTask.Api.Tests;

public sealed class UsageLimiterTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 23, 23, 59, 0, TimeSpan.Zero));

    [Fact]
    public async Task Grants_exactly_up_to_the_limit()
    {
        var limiter = new UsageLimiter(_database.CreateContext(), _clock);

        var granted = 0;
        for (var attempt = 0; attempt < 5; attempt++)
            granted += await limiter.TryConsumeAsync("test", 3, CancellationToken.None) ? 1 : 0;

        Assert.Equal(3, granted);
        Assert.Equal(3, await limiter.CountAsync("test", CancellationToken.None));
    }

    [Fact]
    public async Task Never_grants_a_closed_limit()
    {
        var limiter = new UsageLimiter(_database.CreateContext(), _clock);
        Assert.False(await limiter.TryConsumeAsync("test", 0, CancellationToken.None));
        Assert.False(await limiter.TryConsumeAsync("test", -1, CancellationToken.None));
    }

    [Fact]
    public async Task Counts_separately_per_counter_and_resets_on_the_next_utc_day()
    {
        var limiter = new UsageLimiter(_database.CreateContext(), _clock);
        Assert.True(await limiter.TryConsumeAsync("a", 1, CancellationToken.None));
        Assert.False(await limiter.TryConsumeAsync("a", 1, CancellationToken.None));
        Assert.True(await limiter.TryConsumeAsync("b", 1, CancellationToken.None));

        _clock.Advance(TimeSpan.FromMinutes(2));
        Assert.True(await limiter.TryConsumeAsync("a", 1, CancellationToken.None));
    }

    [Fact]
    public async Task Two_contexts_racing_for_the_last_unit_do_not_both_win()
    {
        var first = new UsageLimiter(_database.CreateContext(), _clock);
        var second = new UsageLimiter(_database.CreateContext(), _clock);
        Assert.True(await first.TryConsumeAsync("shared", 2, CancellationToken.None));

        var results = new[]
        {
            await first.TryConsumeAsync("shared", 2, CancellationToken.None),
            await second.TryConsumeAsync("shared", 2, CancellationToken.None),
        };

        Assert.Single(results, granted => granted);
    }

    public void Dispose() => _database.Dispose();
}
