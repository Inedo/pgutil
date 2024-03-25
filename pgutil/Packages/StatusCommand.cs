using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand
    {
        private sealed class StatusCommand : IConsoleCommandContainer
        {
            public static string Name => "status";
            public static string Description => "Change the status of a package on ProGet";

            public static void Configure(ICommandBuilder builder)
            {
                WithPackageOptions(builder)
                    .WithCommand<ListedCommand>()
                    .WithCommand<BlockCommand>()
                    .WithCommand<DeprecatedCommand>();
            }

            private sealed class ListedCommand : IConsoleCommand
            {
                public static string Name => "listed";
                public static string Description => "List or unlist a package";

                public static void Configure(ICommandBuilder builder) => builder.WithOption<StateOption>();

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var (package, fullName) = GetPackageIdentifier(context);

                    var state = context.GetEnumValue<StateOption, ListedState>();

                    CM.Write("Setting ", new TextSpan($"{fullName} {package.Version}", ConsoleColor.White));
                    if (!string.IsNullOrWhiteSpace(package.Qualifier))
                        CM.Write(ConsoleColor.White, $" {package.Qualifier}");

                    CM.Write(" in ", new TextSpan(package.Feed, ConsoleColor.White), " feed to ");

                    if (state == ListedState.Listed)
                        CM.Write(ConsoleColor.Green, "listed");
                    else
                        CM.Write(ConsoleColor.Yellow, "unlisted");

                    CM.WriteLine("...");

                    await client.SetPackageStatusAsync(package, new PackageStatus(state == ListedState.Listed), cancellationToken);

                    Console.WriteLine("Package status changed.");
                    return 0;
                }

                private sealed class StateOption : IConsoleEnumOption<ListedState>
                {
                    public static bool Required => true;
                    public static string Name => "--state";
                    public static string Description => "Desired listed state of the package.";
                }

                private enum ListedState
                {
                    Listed,
                    Unlisted
                }
            }

            private sealed class BlockCommand : IConsoleCommand
            {
                public static string Name => "block";
                public static string Description => "Blocks or allows a package";

                public static void Configure(ICommandBuilder builder) => builder.WithOption<StateOption>();

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var (package, fullName) = GetPackageIdentifier(context);

                    var state = context.GetEnumValue<StateOption, BlockState>();

                    CM.Write("Setting ", new TextSpan($"{fullName} {package.Version}", ConsoleColor.White));
                    if (!string.IsNullOrWhiteSpace(package.Qualifier))
                        CM.Write(ConsoleColor.White, $" {package.Qualifier}");

                    CM.Write(" in ", new TextSpan(package.Feed, ConsoleColor.White), " feed to ");

                    if (state == BlockState.Blocked)
                        CM.Write(ConsoleColor.Red, "blocked");
                    else if (state == BlockState.Allowed)
                        CM.Write(ConsoleColor.Yellow, "allowed");
                    else
                        CM.Write(ConsoleColor.DarkGray, "(default)");

                    CM.WriteLine("...");

                    await client.SetPackageStatusAsync(
                        package,
                        new PackageStatus(
                            Allow: state switch
                            {
                                BlockState.Blocked => false,
                                BlockState.Allowed => true,
                                _ => null
                            }
                        ), cancellationToken
                    );

                    Console.WriteLine("Package status changed.");
                    return 0;
                }

                private sealed class StateOption : IConsoleEnumOption<BlockState>
                {
                    public static bool Required => true;
                    public static string Name => "--state";
                    public static string Description => "Desired block state of the package.";
                }

                private enum BlockState
                {
                    Default,
                    Blocked,
                    Allowed
                }
            }

            private sealed class DeprecatedCommand : IConsoleCommand
            {
                public static string Name => "deprecated";
                public static string Description => "Mark a package as deprecated or not deprecated.";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<ReasonOption>()
                        .WithOption<ClearFlag>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var (package, fullName) = GetPackageIdentifier(context);

                    bool deprecated = !context.HasFlag<ClearFlag>();
                    string? reason = null;
                    if (deprecated)
                        _ = context.TryGetOption<ReasonOption>(out reason);

                    CM.Write("Setting ", new TextSpan($"{fullName} {package.Version}", ConsoleColor.White));
                    if (!string.IsNullOrWhiteSpace(package.Qualifier))
                        CM.Write(ConsoleColor.White, $" {package.Qualifier}");

                    CM.Write(" in ", new TextSpan(package.Feed, ConsoleColor.White), " feed to ");

                    if (deprecated)
                    {
                        CM.Write(ConsoleColor.Yellow, "deprecated");
                        if (!string.IsNullOrEmpty(reason))
                            CM.Write(ConsoleColor.Yellow, $": {reason}");
                    }
                    else
                    {
                        CM.Write(ConsoleColor.Green, "not deprecated");
                    }

                    CM.WriteLine("...");

                    await client.SetPackageStatusAsync(package, new PackageStatus(Deprecated: deprecated, DeprecatedReason: reason), cancellationToken);

                    Console.WriteLine("Package status changed.");
                    return 0;
                }

                private sealed class ReasonOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--reason";
                    public static string Description => "Reason for deprecating the package";
                }

                private sealed class ClearFlag : IConsoleFlagOption
                {
                    public static string Name => "--clear";
                    public static string Description => "Clears the deprecation flag on the package";
                }
            }
        }
    }
}
