using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand
    {
        private sealed class RepackageCommand : IConsoleCommand
        {
            public static string Name => "repackage";
            public static string Description => "Repackages a package in ProGet to a package with a different version";
            public static string Examples => """
                  $> pgutil packages repackage --feed=public-nuget --package=Newtonsoft.Json --version=13.0.3-beta1 --new-version=13.0.3

                  $> pgutil packages repackage --feed=approved-npm --package=@babel/runtime --version=7.21.4-esm.4 --new-version=7.21.5 

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-packages/proget-api-packages-repackage
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PackageNameOption>()
                    .WithOption<PackageVersionOption>()
                    .WithOption<NewVersionOption>()
                    .WithOption<TargetFeedOption>()
                    .WithOption<CommentOption>();
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
