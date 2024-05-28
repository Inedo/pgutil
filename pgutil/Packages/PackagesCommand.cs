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
    }
}
