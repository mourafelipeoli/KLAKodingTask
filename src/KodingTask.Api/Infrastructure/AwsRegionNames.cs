namespace KodingTask.Api.Infrastructure;

public static class AwsRegionNames
{
    private static readonly Dictionary<string, string> Names = new(StringComparer.OrdinalIgnoreCase)
    {
        ["us-east-1"] = "US East (N. Virginia)",
        ["us-east-2"] = "US East (Ohio)",
        ["us-west-1"] = "US West (N. California)",
        ["us-west-2"] = "US West (Oregon)",
        ["ca-central-1"] = "Canada (Central)",
        ["sa-east-1"] = "South America (São Paulo)",
        ["eu-central-1"] = "Europe (Frankfurt)",
        ["eu-central-2"] = "Europe (Zurich)",
        ["eu-west-1"] = "Europe (Ireland)",
        ["eu-west-2"] = "Europe (London)",
        ["eu-west-3"] = "Europe (Paris)",
        ["eu-north-1"] = "Europe (Stockholm)",
        ["eu-south-1"] = "Europe (Milan)",
        ["ap-northeast-1"] = "Asia Pacific (Tokyo)",
        ["ap-southeast-1"] = "Asia Pacific (Singapore)",
        ["ap-southeast-2"] = "Asia Pacific (Sydney)",
        ["ap-south-1"] = "Asia Pacific (Mumbai)",
    };

    public static string? NameOf(string? region) =>
        region is not null && Names.TryGetValue(region, out var name) ? name : region;
}
