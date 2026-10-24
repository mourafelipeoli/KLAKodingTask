using KodingTask.Api.Accounts;
using KodingTask.Api.Messaging;
using KodingTask.Api.Translations;
using KodingTask.Conversion;
using Microsoft.Extensions.DependencyInjection;

namespace KodingTask.Api.Tests;

public sealed class MessagingUnitTests
{
    private static readonly string[] CompleteLocales =
        ["pt-BR", "de-DE", "fr-FR", "it-IT", "es-ES", "pl-PL", "zh-CN", "zh-TW", "ja-JP", "ko-KR"];

    [Fact]
    public void Email_texts_are_complete_in_every_language_with_the_same_placeholders()
    {
        var server = BakedDictionaries.Load().Server;
        var source = server[TranslationLocales.Source].ToDictionary();
        Assert.All(source, entry => Assert.Equal(entry.Key, entry.Value));

        foreach (var locale in CompleteLocales)
        {
            var dictionary = server[locale].ToDictionary();
            Assert.Equal(source.Keys.Order(), dictionary.Keys.Order());
            foreach (var (key, value) in dictionary)
            {
                Assert.False(string.IsNullOrWhiteSpace(value), $"{locale}: {key}");
                Assert.Equal(BakedDictionaries.PlaceholdersOf(key), BakedDictionaries.PlaceholdersOf(value));
            }
        }

        Assert.All(server["fr-CA"].Select(entry => entry.Key), key => Assert.Contains(key, source.Keys));
    }

    [Fact]
    public void Regional_french_falls_back_to_france_and_unknown_locales_to_english()
    {
        using var factory = new ApiFactory();
        var texts = factory.Services.GetRequiredService<ServerTexts>();

        const string Key = "You received this email because you asked Koding Task to send you this result.";
        Assert.Equal("Vous recevez ce courriel parce que vous avez demandé à Koding Task de vous envoyer ce résultat.", texts.Translate(Key, "fr-CA"));
        Assert.Equal("Vous recevez cet e-mail parce que vous avez demandé à Koding Task de vous envoyer ce résultat.", texts.Translate(Key, "fr-CH"));
        Assert.Equal(Key, texts.Translate(Key, "sv-SE"));
    }

    [Fact]
    public void The_email_encodes_everything_it_did_not_write_itself()
    {
        using var factory = new ApiFactory();
        var composer = new EmailComposer(factory.Services.GetRequiredService<ServerTexts>());

        var email = composer.Result("en-US", "US$ 1.00", "<script>alert(1)</script>", "en-US", "English (US)", "https://example.com/?a=1&b=2");

        Assert.DoesNotContain("<script>", email.Html);
        Assert.Contains("&lt;script&gt;", email.Html);
        Assert.Contains("https://example.com/?a=1&amp;b=2", email.Html);
        Assert.Contains("<script>alert(1)</script>", email.Text);
    }

    [Theory]
    [InlineData("felipe@example.com", "f*****@example.com")]
    [InlineData("a@example.com", "a***@example.com")]
    [InlineData("averyveryverylongname@example.com", "a********@example.com")]
    [InlineData("not-an-address", "***")]
    public void Masks_email_addresses(string address, string masked) => Assert.Equal(masked, DestinationMask.Email(address));

    [Theory]
    [InlineData("+4917012345678", "+49*******5678")]
    [InlineData("+15551234567", "+15*****4567")]
    public void Masks_phone_numbers(string phone, string masked) => Assert.Equal(masked, DestinationMask.Phone(phone));

    [Theory]
    [InlineData("+49 170 1234-5678", "+4917012345678")]
    [InlineData("0049 (170) 12345678", "+4917012345678")]
    [InlineData("+1 (555) 123.4567", "+15551234567")]
    public void Normalises_international_phone_numbers(string input, string expected)
    {
        Assert.True(PhoneNumbers.TryNormalize(input, out var phone));
        Assert.Equal(expected, phone);
    }

    [Theory]
    [InlineData("0170 1234567")]
    [InlineData("+0 123 456 789")]
    [InlineData("+49 170")]
    [InlineData("call me")]
    [InlineData("")]
    public void Refuses_numbers_that_are_not_international(string input) => Assert.False(PhoneNumbers.TryNormalize(input, out _));

    [Theory]
    [InlineData("/?amount=1#send", "/?amount=1#send")]
    [InlineData("/infrastructure", "/infrastructure")]
    [InlineData("//evil.example", "/")]
    [InlineData("/\\evil.example", "/")]
    [InlineData("https://evil.example", "/")]
    [InlineData(null, "/")]
    public void Keeps_return_urls_on_this_site(string? returnUrl, string expected) => Assert.Equal(expected, ReturnUrls.Sanitize(returnUrl));

    [Theory]
    [InlineData("en-US", "US$ 45,100.25")]
    [InlineData("de-DE", "US$ 45.100,25")]
    public void Writes_the_amount_the_way_the_language_writes_numbers(string locale, string expected) =>
        Assert.Equal(expected, AmountText.Format(new MonetaryAmount(45_100, 25), locale));

    [Theory]
    [InlineData("fr-CH", "fr")]
    [InlineData("zh-TW", "zh_TW")]
    [InlineData("es-ES", "es_ES")]
    [InlineData("sv-SE", null)]
    public void Maps_interface_locales_to_whatsapp_template_languages(string locale, string? expected) =>
        Assert.Equal(expected, MetaLanguages.For(locale));
}
