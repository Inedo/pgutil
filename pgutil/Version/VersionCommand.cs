using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    internal sealed class VersionCommand : IConsoleCommand
    {
        static string IConsoleArgument.Name => "version";
        static string IConsoleArgument.Description => "Displays the current version";

        static void IConsoleCommand.Configure(ICommandBuilder builder)
        {
        }
        static Task<int> IConsoleCommand.ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            var currentVersion = typeof(Program).Assembly.GetName().Version!;
            Console.WriteLine(currentVersion.ToString(3));
            return Task.FromResult(0);
        }
    }
}