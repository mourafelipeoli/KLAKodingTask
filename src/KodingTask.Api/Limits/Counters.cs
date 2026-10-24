namespace KodingTask.Api.Limits;

public static class Counters
{
    public const string Emails = "email:sent";

    public const string WhatsAppMessages = "whatsapp:sent";

    public const string WhatsAppMessagesThisMonth = "whatsapp:sent:month";

    public const string TranslationCharacters = "translation:characters";

    public const string TermRegistrations = "terms:registered";

    public static string Deliveries(string userId) => $"deliveries:user:{userId}";

    public static string TermRegistrationsByClient(string clientHash) => $"terms:client:{clientHash}";
}
