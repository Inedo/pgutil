using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    internal sealed partial class ApiKeysCommand
    {
        private sealed partial class CreateCommand
        {
            private sealed class FeedCommand : IConsoleCommand
            {
                public readonly static string[] AvailablePackagePermissions = ["view", "add", "promote", "delete"];
                public static string Name => "feed";
                public static string Description => "Creates a feed API key. The key is the only thing written to stdout on success";

                public static void Configure(ICommandBuilder builder)
                {
                    builder
                        .WithOption<FeedNameOption>()
                        .WithOption<FeedGroupOption>()
                        .WithOption<KeyOption>()
                        .WithOption<NameOption>()
                        .WithOption<DescriptionOption>()
                        .WithOption<ExpirationOption>()
                        .WithOption<LoggingOption>()
                        .WithOption<PackagePermissionsOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    _ = context.TryGetOption<FeedNameOption>(out var feed);
                    _ = context.TryGetOption<FeedGroupOption>(out var group);

                    if ( (feed is not null && group is not null) || (feed is null && group is null) )
                    {
                        CM.WriteError("either --feed or --group must be specified (but not both)");
                        context.WriteUsage();
                        return -1;
                    }

                    var permissions = context.TryGetOption<PackagePermissionsOption>(out var permissionsValue)
                        ? permissionsValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        : FeedCommand.AvailablePackagePermissions;
                    
                    if (permissions.Length == 0 || permissions.Any(p => !FeedCommand.AvailablePackagePermissions.Contains(p)))
                    {
                        CM.WriteError<PackagePermissionsOption>("an invalid value was specified");
                        context.WriteUsage();
                        return -1;
                    }

                    var info = new ApiKeyInfo
                    {
                        Type = ApiKeyType.Feed,
                        Feed = feed,
                        FeedGroup = group,
                        Key = context.GetOptionOrDefault<KeyOption>(),
                        DisplayName = context.GetOptionOrDefault<NameOption>(),
                        Description = context.GetOptionOrDefault<DescriptionOption>(),
                        Expiration = context.TryGetOption<ExpirationOption, DateTime>(out var d) ? d : null,
                        Logging = context.TryGetEnumValue<LoggingOption, ApiKeyBodyLogging>(out var l) ? l : null,
                        PackagePermissions = permissions
                    };

                    var result = await client.CreateApiKeyAsync(info, cancellationToken).ConfigureAwait(false);
                    CM.WriteLine(result.Key);
                    return 0;
                }

                private sealed class PackagePermissionsOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--permissions";
                    public static string Description => 
                        $"Specifies the package permissions to give access to when creating a feed API key. " +
                        $"Value is a comma-separated list of any combination of: {{{string.Join(", ", FeedCommand.AvailablePackagePermissions)}}}";
                }
                private sealed class FeedNameOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--feed";
                    public static string Description => "Name of the feed to associate with the key";
                }
                private sealed class FeedGroupOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--group";
                    public static string Description => "Name of the feed group to associate with the key";
                }

            }
        }
    }
}
