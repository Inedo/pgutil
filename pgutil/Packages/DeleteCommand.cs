using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand
    {
        private sealed class DeleteCommand : IConsoleCommand
        {
            public static string Name => "delete";
            public static string Description => "Deletes a package from ProGet";

            public static void Configure(ICommandBuilder builder) => WithPackageOptions(builder);

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var feedInfo = await client.GetBasicFeedInfoAsync(context.GetFeedName(), cancellationToken);
                var (package, fullName) = GetPackageIdentifier(context);
                QualifierOptions.EnsureQualifiersForPackageType(feedInfo.PackageType, package.Qualifier);

                CM.Write("Deleting ", new TextSpan($"{fullName} {package.Version}", ConsoleColor.White));
                if (!string.IsNullOrWhiteSpace(package.Qualifier))
                    CM.Write(ConsoleColor.White, $" {package.Qualifier}");

                CM.WriteLine(" from ", new TextSpan(package.Feed, ConsoleColor.White), " feed...");

                await client.DeletePackageAsync(package, cancellationToken);

                CM.WriteLine("Package deleted.");

                return 0;
            }
        }
    }
}
