using System.Text.Json;
using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal sealed partial class Program : IConsoleCommandContainer
{
    public static string Name => "pgutil";
    public static string Description => "Perform operations against a ProGet server.";

    public static async Task<int> Main(string[] args)
    {
        var latestVersion = UpdateChecker.GetLatestVersion();

        if (args.Length == 0)
        {
            var currentVersion = typeof(Program).Assembly.GetName().Version;

            Console.Write(
                $"""
                    .--. --. ..- - .. .-.. 
                        pgutil v{currentVersion:3}
                    .--. --. ..- - .. .-.. 


                """);

            if (latestVersion is not null && currentVersion < latestVersion)
            {
                CM.WriteLine(ConsoleColor.Blue, $"pgutil v{latestVersion} is now available.");
                CM.WriteLine();
            }
        }

        try
        {
            return await Command.Create<Program>().ExecuteAsync(args);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or HttpRequestException or ProGetClientException or JsonException)
        {
            CM.WriteError(ex.Message);
            return -1;
        }
    }

    public static void Configure(ICommandBuilder builder)
    {
        builder.WithCommand<SourcesCommand>()
            .WithCommand<ApiKeysCommand>()
            .WithCommand<HealthCommand>()
            .WithCommand<PackagesCommand>()
            .WithCommand<FeedsCommand>()
            .WithCommand<ConnectorsCommand>()
            .WithCommand<BuildsCommand>()            
            .WithCommand<VulnsCommand>()
            .WithCommand<LicensesCommand>()
            .WithCommand<AssetsCommand>()
            .WithCommand<UpackCommand>()
            .WithCommand<SettingsCommand>();
    }
}
