using ConsoleMan;
using Inedo.ProGet;
using PgUtil.Config;

namespace PgUtil;

internal static class Extensions
{
    public static string GetFeedName(this CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TryGetOption<FeedOption>(out var feed))
        {
            CM.WriteError<FeedOption>("Feed must be specified if there is no default feed configured.");
            context.WriteUsage();
            throw new PgUtilException();
        }

        return feed;
    }
    public static ProGetClient GetProGetClient(this CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var apiKey = context.GetOptionOrDefault<ApiKeyOption>();
        var userName = context.GetOptionOrDefault<UserNameOption>();
        var password = context.GetOptionOrDefault<PasswordOption>();

        var sourceName = context.GetOptionOrDefault<SourceOption>() ?? "Default";

        var source = PgUtilConfig.Instance.Sources.FirstOrDefault(s => s.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
        if (source is null)
        {
            if (sourceName.StartsWith("https://") || sourceName.StartsWith("http://"))
            {
                source = new PgUtilSource(sourceName, sourceName);
            }
            else
            {
                CM.WriteError<SourceOption>($"Source {sourceName} not found.");
                throw new PgUtilException();
            }
        }
        else if (!string.IsNullOrEmpty(source.DefaultFeed))
        {
            context.TrySetOption<FeedOption>(source.DefaultFeed);
        }

        // prefer api key
        if (apiKey is not null)
            source = source with { Token = apiKey, Username = null, Password = null };
        else if (userName is not null)
            source = source with { Username = userName, Password = password ?? string.Empty, Token = null };

        return source.GetProGetClient();
    }
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = new List<T>();
        await foreach (var item in source.ConfigureAwait(false))
            list.Add(item);

        return list;
    }
}
