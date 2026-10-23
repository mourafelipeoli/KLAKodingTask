namespace KodingTask.Api.Infrastructure;

public static class AwsArns
{
    public static string? Region(string? arn) =>
        arn?.Split(':') is { Length: > 5 } parts && parts[0] == "arn" && parts[3].Length > 0 ? parts[3] : null;

    public static string? ClusterName(string? cluster)
    {
        if (string.IsNullOrWhiteSpace(cluster))
            return null;
        var marker = cluster.LastIndexOf("cluster/", StringComparison.Ordinal);
        return marker >= 0 ? cluster[(marker + "cluster/".Length)..] : cluster;
    }

    public static string? TargetGroupDimension(string? targetGroupArn) =>
        Suffix(targetGroupArn, "targetgroup/", includeMarker: true);

    public static string? LoadBalancerDimension(string? loadBalancerArn) =>
        Suffix(loadBalancerArn, "loadbalancer/", includeMarker: false);

    private static string? Suffix(string? arn, string marker, bool includeMarker)
    {
        if (string.IsNullOrWhiteSpace(arn))
            return null;
        var index = arn.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0)
            return null;
        return includeMarker ? arn[index..] : arn[(index + marker.Length)..];
    }
}
