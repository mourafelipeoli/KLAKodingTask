using System.Net;
using System.Net.Http.Json;
using KodingTask.Api.Contracts;
using KodingTask.Api.Hosting;
using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KodingTask.Api.Tests;

public sealed class ConversionHistoryTests
{
    private const string History = "/api/account/conversions";

    [Fact]
    public async Task Lists_the_conversions_of_the_signed_in_visitor_newest_first_and_once_each()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync();

        await ConvertAsync(client, "1", "en-US");
        await ConvertAsync(client, "45 100", "de-DE");
        await ConvertAsync(client, "1,00", "en-US");

        var entries = await client.Http.GetFromJsonAsync<List<ConversionHistoryEntry>>(History);
        Assert.Collection(entries!,
            first =>
            {
                Assert.Equal("en-US", first.Language);
                Assert.Equal(1, first.Dollars);
                Assert.Equal("one dollar", first.Words);
            },
            second =>
            {
                Assert.Equal("de-DE", second.Language);
                Assert.Equal(45100, second.Dollars);
                Assert.Equal(0, second.Cents);
                Assert.False(string.IsNullOrWhiteSpace(second.Words));
            });
        Assert.Equal(TimeSpan.Zero, entries![0].ConvertedAt.Offset);
    }

    [Fact]
    public async Task The_same_amount_in_another_language_is_another_entry()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync();

        await ConvertAsync(client, "7", "en-US");
        await ConvertAsync(client, "7", "fr-FR");

        var entries = await client.Http.GetFromJsonAsync<List<ConversionHistoryEntry>>(History);
        Assert.Equal(["fr-FR", "en-US"], entries!.Select(entry => entry.Language));
    }

    [Fact]
    public async Task Keeps_only_the_last_ten()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync();

        for (var dollars = 1; dollars <= 12; dollars++)
            await ConvertAsync(client, dollars.ToString(), "en-US");

        var entries = await client.Http.GetFromJsonAsync<List<ConversionHistoryEntry>>(History);
        Assert.Equal(Enumerable.Range(3, 10).Reverse(), entries!.Select(entry => entry.Dollars));
        Assert.Equal(10, await CountAsync(host));
    }

    [Fact]
    public async Task Nothing_is_kept_for_anonymous_visitors_failed_conversions_or_the_query_endpoint()
    {
        using var host = MessagingTestHost.Create();
        var anonymous = await host.ClientAsync();
        await ConvertAsync(anonymous, "5", "en-US");

        var client = await host.SignedInAccountAsync();
        var failed = await client.PostAsync("/api/conversions", new { amount = "12.50", language = "en-US" });
        var query = await client.Http.GetAsync("/api/conversions?amount=9&language=en-US");

        Assert.Equal(HttpStatusCode.BadRequest, failed.StatusCode);
        Assert.Equal(HttpStatusCode.OK, query.StatusCode);
        Assert.Equal(0, await CountAsync(host));
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.Http.GetAsync(History)).StatusCode);
    }

    [Fact]
    public async Task Each_visitor_sees_only_their_own_list_and_can_clear_it()
    {
        using var host = MessagingTestHost.Create();
        var first = await host.SignedInAccountAsync("first@example.com");
        await ConvertAsync(first, "1", "en-US");
        var second = await host.SignedInAccountAsync("second@example.com");
        await ConvertAsync(second, "2", "en-US");

        var secondList = await second.Http.GetFromJsonAsync<List<ConversionHistoryEntry>>(History);
        Assert.Equal([2], secondList!.Select(entry => entry.Dollars));

        var cleared = await second.Http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, History));
        Assert.Equal(HttpStatusCode.NoContent, cleared.StatusCode);
        Assert.Empty((await second.Http.GetFromJsonAsync<List<ConversionHistoryEntry>>(History))!);
        Assert.Single((await first.Http.GetFromJsonAsync<List<ConversionHistoryEntry>>(History))!);
    }

    [Fact]
    public async Task Clearing_needs_the_antiforgery_header()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync();
        await ConvertAsync(client, "1", "en-US");
        client.Http.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");

        var refused = await client.Http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, History));

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal(1, await CountAsync(host));
    }

    [Fact]
    public async Task The_retention_job_removes_conversions_older_than_ninety_days()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync();
        await ConvertAsync(client, "1", "en-US");
        using (var scope = host.Factory.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>();
            var record = await database.ConversionRecords.SingleAsync();
            database.ConversionRecords.Add(new ConversionRecord
            {
                UserId = record.UserId,
                Amount = "2,00",
                Dollars = 2,
                LanguageCode = "en-US",
                Words = "two dollars",
                ConvertedAtUtc = DateTime.UtcNow - RetentionJob.Retention - TimeSpan.FromDays(1),
            });
            await database.SaveChangesAsync();
        }

        var job = host.Factory.Services.GetServices<IHostedService>().OfType<RetentionJob>().Single();
        await job.PurgeAsync(CancellationToken.None);

        var entries = await client.Http.GetFromJsonAsync<List<ConversionHistoryEntry>>(History);
        Assert.Equal([1], entries!.Select(entry => entry.Dollars));
    }

    private static async Task ConvertAsync(AccountClient client, string amount, string language) =>
        (await client.PostAsync("/api/conversions", new { amount, language })).EnsureSuccessStatusCode();

    private static async Task<int> CountAsync(MessagingTestHost host)
    {
        using var scope = host.Factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>().ConversionRecords.CountAsync();
    }
}
