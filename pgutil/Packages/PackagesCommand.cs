using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand : IConsoleCommandContainer
    {
        public static string Name => "packages";
        public static string Description => "Work with packages on a ProGet server";

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
                .WithCommand<AuditCommand>();
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

        private static (PackageIdentifier package, string fullName) GetPackageIdentifier(CommandContext context)
        {
            var fullName = context.GetOption<PackageNameOption>();
            var version = context.GetOption<PackageVersionOption>();
            var qualifier = context.GetOptionOrDefault<PackageQualifierOption>();
            string? group = null;
            var name = fullName;

            int index = fullName.LastIndexOf('/');
            if (index >= 0)
            {
                group = fullName[..index];
                name = fullName[(index + 1)..];
            }

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

        private sealed class RepackageCommand : IConsoleCommand
        {
            public static string Name => "repackage";
            public static string Description => "Repackages a package in ProGet to a package with a different version";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PackageNameOption>()
                    .WithOption<PackageVersionOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                var (p, fullName) = GetPackageIdentifier(context);

                var repack = new RepackageInput
                {
                    Feed = p.Feed,
                    Name = p.Name,
                    Group = p.Group,
                    Version = p.Version,
                    NewVersion = context.GetOption<NewVersionOption>(),
                    ToFeed = context.TryGetOption<TargetFeedOption>(out var newFeed) ? newFeed : null,
                    Comments = context.TryGetOption<CommentOption>(out var comment) ? comment : null
                };

                CM.WriteLine("Repackaging ", new TextSpan($"{fullName} {repack.Version}", ConsoleColor.White));
                CM.WriteLine("  from ", new TextSpan(repack.Feed, ConsoleColor.White));
                CM.WriteLine("  to ", new TextSpan(repack.ToFeed ?? repack.Feed, ConsoleColor.White), ": ", new TextSpan(repack.NewVersion, ConsoleColor.White));

                await client.RepackageAsync(repack, cancellationToken);

                Console.WriteLine("Repackage successful.");
                return 0;
            }

            private sealed class NewVersionOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--new-version";
                public static string Description => "New version of the package";
            }

            private sealed class TargetFeedOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--to-feed";
                public static string Description => "When specified, adds the new package to this feed instead of the original feed";
            }

            private sealed class CommentOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--comment";
                public static string Description => "Optional comment to record with the repackage history";
            }
        }
    }
}
