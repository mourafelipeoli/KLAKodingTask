namespace KodingTask.Api.Translations;

public static class LocaleResolver
{
    public const string Cookie = "kodingtask-lang";

    public static string Resolve(HttpContext context)
    {
        var request = context.Request;
        if (TranslationLocales.Match(request.Query["lang"]) is { } fromQuery)
            return fromQuery;
        if (TranslationLocales.Match(request.Cookies[Cookie]) is { } fromCookie)
            return fromCookie;
        foreach (var language in request.GetTypedHeaders().AcceptLanguage.OrderByDescending(value => value.Quality ?? 1))
        {
            if (TranslationLocales.Match(language.Value.Value) is { } fromHeader)
                return fromHeader;
        }

        return TranslationLocales.Source;
    }
}
