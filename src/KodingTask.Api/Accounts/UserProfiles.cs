using KodingTask.Conversion;
using KodingTask.Data;
using KodingTask.Data.Entities;

namespace KodingTask.Api.Accounts;

public sealed class UserProfiles(KodingTaskDbContext database, SpellerCatalog catalog, TimeProvider timeProvider)
{
    public const string DefaultLocale = "en-US";

    public async Task<string> RememberLocaleAsync(string userId, string? uiLocale, CancellationToken cancellationToken)
    {
        var profile = await database.UserProfiles.FindAsync([userId], cancellationToken);
        var locale = catalog.TryGet(uiLocale, out var speller) ? speller.Language.Code : profile?.PreferredLocale ?? DefaultLocale;
        if (profile is null)
        {
            database.UserProfiles.Add(new UserProfile
            {
                UserId = userId,
                PreferredLocale = locale,
                CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
            });
        }
        else
        {
            profile.PreferredLocale = locale;
        }

        await database.SaveChangesAsync(cancellationToken);
        return locale;
    }
}
