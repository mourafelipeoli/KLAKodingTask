using System.Net;

namespace KodingTask.Api.Messaging;

public sealed record ComposedEmail(string Subject, string Html, string Text);

public sealed class EmailComposer(MessageTexts texts)
{
    private const string Brand = "KODING TASK";
    private const string Tagline = "Keep Coding Ahead";

    public ComposedEmail Result(string locale, string amountText, string words, string wordsLocale, string languageName, string link)
    {
        string T(string key) => texts.Translate(locale, key);
        var subject = texts.Translate(locale, "{{amount}} in words", new Dictionary<string, string> { ["amount"] = amountText });
        var html = Page(locale, subject, $"""
            <p style="margin:0 0 20px;color:#5a5a64;font-size:16px;line-height:1.5;">{Encode(T("Here is the amount you converted on Koding Task."))}</p>
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;margin:0 0 24px;">
              {Row(T("Amount"), Encode(amountText), locale)}
              {Row(T("In words"), Encode(words), wordsLocale, emphasise: true)}
              {Row(T("Language"), Encode(languageName), locale)}
            </table>
            {Button(T("Open this conversion"), link)}
            """, T("You received this email because you asked Koding Task to send you this result."));
        var text = string.Join(Environment.NewLine,
            subject,
            string.Empty,
            T("Here is the amount you converted on Koding Task."),
            string.Empty,
            $"{T("Amount")}: {amountText}",
            $"{T("In words")}: {words}",
            $"{T("Language")}: {languageName}",
            string.Empty,
            $"{T("Open this conversion")}: {link}",
            string.Empty,
            T("You received this email because you asked Koding Task to send you this result."));
        return new ComposedEmail(subject, html, text);
    }

    private static string Page(string locale, string heading, string content, string footer) => $"""
        <!doctype html>
        <html lang="{Encode(locale)}">
        <head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>{Encode(heading)}</title></head>
        <body style="margin:0;padding:0;background:#f6f6f8;font-family:'Segoe UI',Helvetica,Arial,sans-serif;color:#0e0d14;">
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f6f6f8;padding:32px 16px;">
            <tr><td align="center">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:560px;background:#ffffff;border-top:4px solid #5b2a86;">
                <tr><td style="padding:28px 32px 0;">
                  <div style="font-size:20px;letter-spacing:0.18em;color:#5b2a86;">{Brand}</div>
                  <div style="font-size:12px;color:#5b2a86;">{Tagline}</div>
                </td></tr>
                <tr><td style="padding:24px 32px 32px;">
                  <h1 style="margin:0 0 16px;font-size:24px;font-weight:300;color:#5b2a86;">{Encode(heading)}</h1>
                  {content}
                </td></tr>
              </table>
              <p style="max-width:560px;margin:16px auto 0;color:#73737d;font-size:12px;line-height:1.5;">{Encode(footer)}</p>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    private static string Row(string label, string encodedValue, string valueLocale, bool emphasise = false) => $"""
        <tr>
          <td style="padding:10px 12px 10px 0;border-bottom:1px solid #dedee3;color:#5a5a64;font-size:14px;vertical-align:top;white-space:nowrap;">{Encode(label)}</td>
          <td lang="{Encode(valueLocale)}" style="padding:10px 0;border-bottom:1px solid #dedee3;font-size:{(emphasise ? "18px;color:#5b2a86" : "14px;color:#0e0d14")};">{encodedValue}</td>
        </tr>
        """;

    private static string Button(string label, string link) => $"""
        <a href="{Encode(link)}" style="display:inline-block;padding:12px 24px;background:#0b6fb3;color:#ffffff;font-weight:600;text-decoration:none;">{Encode(label)}</a>
        """;

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
