namespace KodingTask.Api.Messaging;

public static class DestinationMask
{
    public static string Email(string address)
    {
        var at = address.IndexOf('@');
        if (at <= 0)
            return "***";
        return $"{address[0]}{new string('*', Math.Clamp(at - 1, 3, 8))}{address[at..]}";
    }

    public static string Phone(string phoneE164)
    {
        if (phoneE164.Length < 8)
            return "***";
        return $"{phoneE164[..3]}{new string('*', phoneE164.Length - 7)}{phoneE164[^4..]}";
    }
}
