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
            public static string Examples => """
                  $> pgutil packages delete --feed=approved-npm --package=@babel/runtime --version=7.25.0

                  $> pgutil packages delete --feed=public-pypi --package=Django --version=5.0.6 --filename=Django-5.0.6.tar.gz

                  $> pgutil packages delete --feed=private-debian --package=debhelper --version=13.15.3 --component=main --distro=stable --arch=all

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-packages/proget-api-packages-delete
                """;

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
