using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KodingTask.Api.Contracts;
using KodingTask.Api.Hosting;
using KodingTask.Api.Translations;
using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KodingTask.Api.Tests;

public sealed class TranslationTests
{
    [Fact]
    public void Every_language_is_declared_once_with_the_deepl_target_it_needs()
    {
        var languages = TranslationLocales.Languages;

        Assert.Equal(languages.Count, languages.Select(language => language.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal("DE", TranslationLocales.MachineTargets["de-DE"]);
        Assert.Equal("PT-BR", TranslationLocales.MachineTargets["pt-BR"]);
        Assert.Equal("ZH-HANS", TranslationLocales.MachineTargets["zh-CN"]);
        Assert.Equal("ZH-HANT", TranslationLocales.MachineTargets["zh-TW"]);
        Assert.False(TranslationLocales.MachineTargets.ContainsKey("en-US"));
        Assert.Equal("fr-FR", TranslationLocales.Derived["fr-CA"]);
        Assert.False(TranslationLocales.MachineTargets.ContainsKey("fr-CA"));
        Assert.All(BakedDictionaries.Load().Interface.Keys, locale => Assert.NotNull(TranslationLocales.Canonical(locale)));
    }

    [Fact]
    public async Task Importing_the_bundled_dictionaries_is_idempotent_and_never_overwrites_a_hand_edit()
    {
        using var host = MessagingTestHost.Create();
        using var scope = host.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<TranslationSeeder>();
        var baked = scope.ServiceProvider.GetRequiredService<BakedDictionaries>();

        Assert.True(await seeder.SeedAsync(CancellationToken.None) > 0);
        Assert.Equal(baked.SourceKeys.Count, await database.Terms.CountAsync());
        Assert.Equal(0, await seeder.SeedAsync(CancellationToken.None));

        var signIn = await GermanAsync(database, "Sign in");
        signIn.Text = "Einloggen";
        signIn.Source = TranslationSource.Manual;
        var signOut = await GermanAsync(database, "Sign out");
        signOut.Text = "machine guess";
        signOut.Source = TranslationSource.Machine;
        await database.SaveChangesAsync();

        await seeder.SeedAsync(CancellationToken.None);
        database.ChangeTracker.Clear();

        Assert.Equal("Einloggen", (await GermanAsync(database, "Sign in")).Text);
        var restored = await GermanAsync(database, "Sign out");
        Assert.Equal("Abmelden", restored.Text);
        Assert.Equal(TranslationSource.Baked, restored.Source);
        Assert.True(restored.Reviewed);
    }

    [Fact]
    public async Task A_language_without_any_translation_gets_every_text_from_the_machine()
    {
        var translator = new FakeTranslator();
        using var host = Create(translator);
        await SeedAsync(host);
        await ForgetAsync(host, "ko-KR");

        var translated = await RunBatchAsync(host);

        using var check = host.Factory.Services.CreateScope();
        var db = check.ServiceProvider.GetRequiredService<KodingTaskDbContext>();
        var terms = await db.Terms.CountAsync(term => term.IsActive);
        var korean = await db.Translations.Where(translation => translation.Locale == "ko-KR").ToListAsync();
        Assert.Equal(terms, translated);
        Assert.Equal(terms, korean.Count);
        Assert.All(korean, row => Assert.Equal(TranslationSource.Machine, row.Source));
        Assert.All(korean, row => Assert.False(row.Reviewed));
        Assert.All(translator.Calls, call => Assert.Equal("ko-KR", call.Locale));
        Assert.All(translator.Calls, call => Assert.InRange(call.Texts.Count, 1, 50));

        var dictionary = await host.Browser().GetFromJsonAsync<TranslationDictionaryResponse>("/api/i18n/ko-KR");
        Assert.Equal("[ko-KR] Sign in", dictionary!.Entries["Sign in"]);
    }

    [Fact]
    public async Task The_daily_character_allowance_and_the_kill_switch_stop_the_machine()
    {
        var translator = new FakeTranslator();
        using var host = Create(translator, settings => settings["Limits:TranslationCharactersPerDay"] = "5");
        await SeedAsync(host);
        await RegisterAsync(host, "A text far longer than five characters");

        Assert.Equal(0, await RunBatchAsync(host));
        Assert.Empty(translator.Calls);

        var idle = new FakeTranslator();
        using var paused = Create(idle, settings => settings["Limits:KillSwitch"] = "true");
        await SeedAsync(paused);
        await ForgetAsync(paused, "ko-KR");
        Assert.Equal(0, await RunBatchAsync(paused));
        Assert.Empty(idle.Calls);
    }

    [Fact]
    public async Task The_dictionary_is_versioned_and_regional_french_inherits_from_france()
    {
        using var host = Create(new FakeTranslator());
        await SeedAsync(host);
        var browser = host.Browser();

        var german = await browser.GetAsync("/api/i18n/de-DE");
        var body = await german.Content.ReadFromJsonAsync<TranslationDictionaryResponse>();
        Assert.Equal("Anmelden", body!.Entries["Sign in"]);
        Assert.Equal($"\"{body.Version}\"", german.Headers.ETag!.Tag);

        using var revalidation = new HttpRequestMessage(HttpMethod.Get, "/api/i18n/de-DE");
        revalidation.Headers.IfNoneMatch.Add(german.Headers.ETag);
        Assert.Equal(HttpStatusCode.NotModified, (await browser.SendAsync(revalidation)).StatusCode);

        var canadian = await browser.GetFromJsonAsync<TranslationDictionaryResponse>("/api/i18n/fr-CA");
        Assert.Contains("courriel", canadian!.Entries["Email"], StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Se connecter", canadian.Entries["Sign in"]);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync("/api/i18n/xx-XX")).StatusCode);
    }

    [Fact]
    public async Task The_language_comes_from_the_address_then_the_cookie_then_the_browser()
    {
        using var host = Create(new FakeTranslator());
        var browser = host.Browser();

        using var byHeader = new HttpRequestMessage(HttpMethod.Get, "/api/i18n");
        byHeader.Headers.AcceptLanguage.ParseAdd("de-AT,de;q=0.9,en;q=0.5");
        Assert.Equal("de-DE", (await (await browser.SendAsync(byHeader)).Content.ReadFromJsonAsync<TranslationDictionaryResponse>())!.Locale);

        using var byCookie = new HttpRequestMessage(HttpMethod.Get, "/api/i18n");
        byCookie.Headers.Add("Cookie", "kodingtask-lang=it-IT");
        byCookie.Headers.AcceptLanguage.ParseAdd("de-DE");
        Assert.Equal("it-IT", (await (await browser.SendAsync(byCookie)).Content.ReadFromJsonAsync<TranslationDictionaryResponse>())!.Locale);

        var byAddress = await browser.GetFromJsonAsync<TranslationDictionaryResponse>("/api/i18n?lang=ja-JP");
        Assert.Equal("ja-JP", byAddress!.Locale);

        var languages = await browser.GetFromJsonAsync<List<InterfaceLanguageResponse>>("/api/i18n/languages");
        Assert.NotNull(languages);
        Assert.Equal(TranslationLocales.All, languages.Select(language => language.Code));
        Assert.Equal("fr-FR", languages.Single(language => language.Code == "fr-CA").VariantOf);
    }

    [Fact]
    public async Task Unknown_texts_from_the_page_become_terms_within_their_caps()
    {
        using var host = Create(new FakeTranslator(), settings => settings["Limits:TermRegistrationsPerClientPerDay"] = "2");
        await SeedAsync(host);
        var page = await host.ClientAsync();

        var first = await page.PostAsync("/api/i18n/terms", new { texts = new[] { "A new sentence", "A new sentence", "Sign in" } });
        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(1, (await first.Content.ReadFromJsonAsync<TermRegistrationResponse>())!.Added);

        var tooLong = await page.PostAsync("/api/i18n/terms", new { texts = new[] { new string('a', 301) } });
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);

        Assert.Equal(1, (await (await page.PostAsync("/api/i18n/terms", new { texts = new[] { "Second sentence" } })).Content.ReadFromJsonAsync<TermRegistrationResponse>())!.Added);
        var capped = await page.PostAsync("/api/i18n/terms", new { texts = new[] { "Third sentence" } });
        Assert.Equal(HttpStatusCode.TooManyRequests, capped.StatusCode);

        var forged = await host.Browser().PostAsJsonAsync("/api/i18n/terms", new { texts = new[] { "Forged sentence" } });
        Assert.Equal(HttpStatusCode.BadRequest, forged.StatusCode);

        var english = await page.Http.GetFromJsonAsync<TranslationDictionaryResponse>("/api/i18n/en-US");
        Assert.True(english!.Entries.ContainsKey("A new sentence"));
        Assert.False(english.Entries.ContainsKey("Third sentence"));
    }

    [Fact]
    public async Task Server_texts_it_does_not_know_become_terms_and_error_messages_follow_the_language()
    {
        using var host = Create(new FakeTranslator());
        await SeedAsync(host);
        var texts = host.Factory.Services.GetRequiredService<ServerTexts>();
        var index = host.Factory.Services.GetRequiredService<TranslationIndex>();
        await index.GetAsync(TranslationLocales.Source, CancellationToken.None);

        Assert.Equal("A sentence only the server uses", texts.Translate("A sentence only the server uses", "de-DE"));
        using (var scope = host.Factory.Services.CreateScope())
        {
            var pending = scope.ServiceProvider.GetRequiredService<PendingTerms>().Drain();
            await scope.ServiceProvider.GetRequiredService<TermRegistry>().AddServerTermsAsync(pending, CancellationToken.None);
            var database = scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>();
            Assert.True(await database.Terms.AnyAsync(term => term.Text == "A sentence only the server uses" && term.Origin == TermOrigin.Server));
        }

        const string Message = "Only digits, spaces and one comma are allowed.";
        Assert.Equal("Erlaubt sind nur Ziffern, Leerzeichen und ein Komma.", await RefusalInGermanAsync(host));

        using (var scope = host.Factory.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>();
            var german = await database.Translations.SingleAsync(translation => translation.Locale == "de-DE" && translation.Term!.Text == Message);
            german.Text = "Nur Ziffern, Leerzeichen und ein Komma sind erlaubt.";
            german.Source = TranslationSource.Manual;
            await database.SaveChangesAsync();
        }

        index.Invalidate();
        await index.GetAsync("de-DE", CancellationToken.None);
        Assert.Equal("Nur Ziffern, Leerzeichen und ein Komma sind erlaubt.", await RefusalInGermanAsync(host));
    }

    private static async Task<string?> RefusalInGermanAsync(MessagingTestHost host)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/conversions?amount=12x&language=en-US");
        request.Headers.Add("Cookie", "kodingtask-lang=de-DE");
        var refused = await host.Browser().SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        using var body = JsonDocument.Parse(await refused.Content.ReadAsStringAsync());
        Assert.Equal("InvalidCharacter", body.RootElement.GetProperty("error").GetString());
        return body.RootElement.GetProperty("message").GetString();
    }

    [Fact]
    public async Task A_refusal_from_an_authorization_filter_follows_the_language_too()
    {
        const string German = "Der Anfrage fehlt ein gültiges Antiforgery-Token. Laden Sie die Seite neu und versuchen Sie es noch einmal.";
        using var host = Create(new FakeTranslator());
        await SeedAsync(host);
        var message = ValidateAntiforgeryHeaderAttribute.Invalid.Message;
        using (var scope = host.Factory.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>();
            var term = new Term { Text = message, TextHash = TermText.Hash(message), Origin = TermOrigin.Server, IsActive = true };
            term.Translations.Add(new Translation { Locale = "de-DE", Text = German, Source = TranslationSource.Manual, Reviewed = true });
            database.Terms.Add(term);
            await database.SaveChangesAsync();
        }

        var index = host.Factory.Services.GetRequiredService<TranslationIndex>();
        index.Invalidate();
        await index.GetAsync("de-DE", CancellationToken.None);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/i18n/terms") { Content = JsonContent.Create(new { texts = new[] { "A sentence" } }) };
        request.Headers.Add("Cookie", "kodingtask-lang=de-DE");
        var refused = await host.Browser().SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        using var body = JsonDocument.Parse(await refused.Content.ReadAsStringAsync());
        Assert.Equal("InvalidAntiforgeryToken", body.RootElement.GetProperty("error").GetString());
        Assert.Equal(German, body.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Only_administrators_review_and_export_translations()
    {
        using var host = Create(new FakeTranslator(), settings => settings["Admin:Emails:0"] = "admin@example.com");
        await SeedAsync(host);

        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Browser().GetAsync("/api/admin/translations?locale=de-DE")).StatusCode);
        var visitor = await host.SignedInAccountAsync("visitor@example.com");
        Assert.Equal(HttpStatusCode.Forbidden, (await visitor.Http.GetAsync("/api/admin/translations?locale=de-DE")).StatusCode);
        Assert.False((await visitor.Http.GetFromJsonAsync<AccountResponse>("/api/account/me"))!.IsAdmin);

        var admin = await host.SignedInAccountAsync("admin@example.com");
        Assert.True((await admin.Http.GetFromJsonAsync<AccountResponse>("/api/account/me"))!.IsAdmin);
        var rows = await admin.Http.GetFromJsonAsync<List<AdminTranslationRow>>("/api/admin/translations?locale=de-DE");
        var signIn = rows!.Single(row => row.English == "Sign in");
        Assert.Equal("Anmelden", signIn.Text);
        Assert.Equal("Baked", signIn.Source);

        var saved = await admin.Http.PutAsJsonAsync("/api/admin/translations", new { termId = signIn.TermId, locale = "de-DE", text = "Einloggen", reviewed = true });
        var row = await saved.Content.ReadFromJsonAsync<AdminTranslationRow>();
        Assert.Equal("Manual", row!.Source);
        Assert.True(row.Reviewed);

        using (var scope = host.Factory.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<TranslationSeeder>().SeedAsync(CancellationToken.None);
        Assert.Equal("Einloggen", (await host.Browser().GetFromJsonAsync<TranslationDictionaryResponse>("/api/i18n/de-DE"))!.Entries["Sign in"]);

        var export = await admin.Http.GetAsync("/api/admin/translations/export/de-DE");
        Assert.Equal("de-DE.json", export.Content.Headers.ContentDisposition!.FileName?.Trim('"'));
        var exported = JsonSerializer.Deserialize<Dictionary<string, string>>(await export.Content.ReadAsStringAsync());
        Assert.Equal("Einloggen", exported!["Sign in"]);

        var run = await admin.PostAsync("/api/admin/translations/run", new { });
        Assert.Equal(HttpStatusCode.ServiceUnavailable, run.StatusCode);
    }

    private static MessagingTestHost Create(FakeTranslator translator, Action<Dictionary<string, string>>? configure = null) =>
        MessagingTestHost.Create(configure, services =>
        {
            services.RemoveAll<IMachineTranslator>();
            services.AddSingleton<IMachineTranslator>(translator);
        });

    private static async Task SeedAsync(MessagingTestHost host)
    {
        using var scope = host.Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<TranslationSeeder>().SeedAsync(CancellationToken.None);
        scope.ServiceProvider.GetRequiredService<TranslationIndex>().Invalidate();
    }

    private static async Task ForgetAsync(MessagingTestHost host, string locale)
    {
        using var scope = host.Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>().Translations.Where(translation => translation.Locale == locale).ExecuteDeleteAsync();
        scope.ServiceProvider.GetRequiredService<TranslationIndex>().Invalidate();
    }

    private static async Task<int> RunBatchAsync(MessagingTestHost host)
    {
        using var scope = host.Factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<TranslationBatch>().RunAsync(CancellationToken.None);
    }

    private static async Task RegisterAsync(MessagingTestHost host, string text)
    {
        using var scope = host.Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<TermRegistry>().RegisterAsync([text], "test-client", CancellationToken.None);
    }

    private static async Task<Translation> GermanAsync(KodingTaskDbContext database, string english) =>
        await database.Translations.SingleAsync(translation => translation.Locale == "de-DE" && translation.Term!.Text == english);
}
