using Amazon;
using Amazon.SimpleEmailV2;

namespace KodingTask.Api.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static (bool Email, bool WhatsApp) AddMessaging(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var emailSection = configuration.GetSection(EmailOptions.SectionName);
        var whatsAppSection = configuration.GetSection(WhatsAppOptions.SectionName);
        services.Configure<EmailOptions>(emailSection);
        services.Configure<WhatsAppOptions>(whatsAppSection);
        services.AddSingleton<MessageTexts>();
        services.AddSingleton<EmailComposer>();
        services.AddScoped<MessageLog>();

        var email = emailSection.Get<EmailOptions>() ?? new EmailOptions();
        var whatsApp = whatsAppSection.Get<WhatsAppOptions>() ?? new WhatsAppOptions();
        if ((email.IsLogMode || whatsApp.IsLogMode) && !environment.IsDevelopment())
            throw new InvalidOperationException("Email:Mode=Log and WhatsApp:Mode=Log are only allowed in the Development environment.");

        if (email.IsConfigured && email.IsLogMode)
        {
            services.AddSingleton<IEmailSender, LogEmailSender>();
        }
        else if (email.IsConfigured)
        {
            services.AddSingleton<IAmazonSimpleEmailServiceV2>(_ =>
                new AmazonSimpleEmailServiceV2Client(RegionEndpoint.GetBySystemName(email.Region)));
            services.AddSingleton<IEmailSender, SesEmailSender>();
        }
        else
        {
            services.AddSingleton<IEmailSender, UnavailableEmailSender>();
        }

        if (whatsApp.IsLogMode)
            services.AddSingleton<IWhatsAppSender, LogWhatsAppSender>();
        else if (whatsApp.IsConfigured)
            services.AddHttpClient<IWhatsAppSender, MetaWhatsAppSender>(client => client.Timeout = TimeSpan.FromSeconds(whatsApp.TimeoutSeconds));
        else
            services.AddSingleton<IWhatsAppSender, UnavailableWhatsAppSender>();

        return (email.IsConfigured, whatsApp.IsConfigured);
    }
}
