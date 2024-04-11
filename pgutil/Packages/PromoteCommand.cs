using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand
    {
        private sealed class PromoteCommand : IConsoleCommand
        {
            public static string Name => "promote";
            public static string Description => "Promotes a package from one feed to another";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PackageNameOption>()
                    .WithOption<PackageVersionOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                var (p, fullName) = GetPackageIdentifier(context);

                var repack = new PromotePackageInput
                {
                    FromFeed = p.Feed,
                    Name = p.Name,
                    Group = p.Group,
                    Version = p.Version,
                    ToFeed = context.GetOption<TargetFeedOption>(),
                    Comments = context.TryGetOption<CommentOption>(out var comment) ? comment : null
                };

                CM.WriteLine("Promoting ", new TextSpan($"{fullName} {repack.Version}", ConsoleColor.White));
                CM.WriteLine("  from ", new TextSpan(repack.FromFeed, ConsoleColor.White));
                CM.WriteLine("  to ", new TextSpan(repack.ToFeed, ConsoleColor.White));

                await client.PromotePackageAsync(repack, cancellationToken);

                Console.WriteLine("Promote successful.");
                return 0;
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
