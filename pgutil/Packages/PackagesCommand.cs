using System.Diagnostics.CodeAnalysis;
using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand : IConsoleCommandContainer
    {
        public static string Name => "packages";
        public static string Description => "Work with packages on a ProGet server";

        private sealed class LatestCommand : IConsoleCommand
        {
            public static string Name => "latest";
            public static string Description => "Display latest versions of packages in a feed";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PackageNameOption>(false);
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var feed = context.GetFeedName();

                _ = TryGetPackageName(context, out _, out var name, out var group);

                await foreach (var p in client.ListLatestPackagesAsync(feed, name, group, cancellationToken))
                    Console.WriteLine($"{p.Name} {p.Version}");

                return 0;
            }
        }

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<SourceOption>()
                .WithOption<ApiKeyOption>()
                .WithOption<UserNameOption>()
                .WithOption<PasswordOption>()
                .WithOption<FeedOption>()
                .WithOption<NoConnectorsFlag>()
                .WithCommand<DownloadCommand>()
                .WithCommand<UploadCommand>()
                .WithCommand<DeleteCommand>()
                .WithCommand<StatusCommand>()
                .WithCommand<RepackageCommand>()
                .WithCommand<PromoteCommand>()
                .WithCommand<AuditCommand>()
                .WithCommand<LatestCommand>();
        }

        private sealed class PackageNameOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--package";
            public static string Description => "Name (and group where applicable) of package";
        }

        private sealed class PackageVersionOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--version";
            public static string Description => "Version of package";
        }

        private sealed class PackageQualifierOption : IConsoleOption
        {
            public static bool Required => false;
            public static string Name => "--qualifier";
            public static string Description => "Qualifier used by multifile packages like Debian and RubyGems";
        }

        private sealed class NoConnectorsFlag : IConsoleFlagOption
        {
            public static string Name => "--no-connectors";
            public static string Description => "Only include local (non-connector) package data in results";
        }

        private static ICommandBuilder WithPackageOptions(ICommandBuilder builder)
        {
            return builder.WithOption<PackageNameOption>()
                .WithOption<PackageVersionOption>()
                .WithOption<PackageQualifierOption>();
        }

        private static bool TryGetPackageName(CommandContext context, [MaybeNullWhen(false)] out string fullName, [MaybeNullWhen(false)] out string name, out string? group)
        {
            name = null;
            group = null;
            if (!context.TryGetOption<PackageNameOption>(out fullName))
                return false;

            int index = fullName.LastIndexOf('/');
            if (index >= 0)
            {
                group = fullName[..index];
                name = fullName[(index + 1)..];
            }
            else
            {
                name = fullName;
            }

            return true;
        }
        private static (PackageIdentifier package, string fullName) GetPackageIdentifier(CommandContext context)
        {
            if (!TryGetPackageName(context, out var fullName, out var name, out var group))
            {
                CM.WriteError<PackageNameOption>("missing required argument");
                throw new PgUtilException();
            }

            var version = context.GetOption<PackageVersionOption>();
            var qualifier = context.GetOptionOrDefault<PackageQualifierOption>();

            var feed = context.GetFeedName();

            return (new PackageIdentifier
            {
                Feed = feed,
                Name = name,
                Group = group,
                Version = version,
                Qualifier = qualifier
            }, fullName);
        }
    }
}
