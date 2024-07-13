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
        if (args.Length == 0)
        {
            Console.Write(
                $"""
                    .--. --. ..- - .. .-.. 
                        pgutil v{typeof(Program).Assembly.GetName().Version:3}
                    .--. --. ..- - .. .-.. 


                """);
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
            .WithCommand<HealthCommand>()
            .WithCommand<PackagesCommand>()
            .WithCommand<VulnsCommand>()
            .WithCommand<BuildsCommand>()
            .WithCommand<LicensesCommand>()
            .WithCommand<ApiKeysCommand>()
            .WithCommand<AssetsCommand>()
            .WithCommand<SettingsCommand>()
            .WithCommand<FeedsCommand>()
            .WithCommand<ConnectorsCommand>();
    }
}
