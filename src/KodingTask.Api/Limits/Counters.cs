namespace KodingTask.Api.Limits;

public static class Counters
{
    public const string Emails = "email:sent";

    public const string WhatsAppMessages = "whatsapp:sent";

    public static string Deliveries(string userId) => $"deliveries:user:{userId}";

    public static string PhoneCodes(string userId) => $"phone-code:user:{userId}";

    public static string AccountEmails(string userId) => $"account-email:user:{userId}";

    public static string Registrations(string clientHash) => $"register:client:{clientHash}";
}
