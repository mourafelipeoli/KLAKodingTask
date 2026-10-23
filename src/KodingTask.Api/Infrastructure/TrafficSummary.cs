namespace KodingTask.Api.Infrastructure;

public readonly record struct MetricPoint(DateTimeOffset At, double Value);

public static class TrafficSummary
{
    public const int Hours = 24;

    public static TrafficFacts? Summarise(
        DateTimeOffset firstHour,
        DateTimeOffset since,
        IReadOnlyList<MetricPoint>? requests,
        IReadOnlyList<MetricPoint>? responseSeconds,
        IReadOnlyList<MetricPoint>? cpuPercent,
        IReadOnlyList<MetricPoint>? memoryPercent)
    {
        if (requests is null && responseSeconds is null && cpuPercent is null && memoryPercent is null)
            return null;

        long[]? perHour = null;
        double? averageResponseMs = null;
        if (requests is not null)
        {
            perHour = new long[Hours];
            foreach (var point in requests)
            {
                if (HourIndex(firstHour, point.At) is { } index)
                    perHour[index] += (long)Math.Round(point.Value);
            }

            averageResponseMs = WeightedAverageMs(firstHour, perHour, responseSeconds);
        }

        return new TrafficFacts(
            perHour?.Sum(),
            perHour,
            averageResponseMs,
            AverageSince(cpuPercent, since),
            AverageSince(memoryPercent, since));
    }

    private static double? WeightedAverageMs(DateTimeOffset firstHour, long[] perHour, IReadOnlyList<MetricPoint>? responseSeconds)
    {
        if (responseSeconds is null)
            return null;

        double weighted = 0;
        long requests = 0;
        foreach (var point in responseSeconds)
        {
            if (HourIndex(firstHour, point.At) is not { } index || perHour[index] == 0)
                continue;
            weighted += point.Value * perHour[index];
            requests += perHour[index];
        }

        return requests == 0 ? null : Math.Round(weighted / requests * 1000, 1);
    }

    private static double? AverageSince(IReadOnlyList<MetricPoint>? points, DateTimeOffset since)
    {
        var recent = points?.Where(point => point.At >= since).Select(point => point.Value).ToList();
        return recent is { Count: > 0 } ? Math.Round(recent.Average(), 1) : null;
    }

    private static int? HourIndex(DateTimeOffset firstHour, DateTimeOffset at)
    {
        var index = (int)Math.Round((at - firstHour).TotalHours);
        return index is >= 0 and < Hours ? index : null;
    }
}
