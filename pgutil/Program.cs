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
                        pgutil v{typeof(Program).Assembly.GetName().Version!.ToString(3)}
                    .--. --. ..- - .. .-.. 


                """);
        }

        try
        {
            return await Command.Create<Program>().ExecuteAsync(args);
        }
        catch (HttpRequestException ex)
        {
            CM.WriteError(ex.Message);
            return -1;
        }
        catch (ProGetClientException ex)
        {
            CM.WriteError(ex.Message);
            return -1;
        }
        catch (PgUtilException ex)
        {
            if (ex.HasMessage)
                CM.WriteError(ex.Message);
            return -1;
        }
    }

    public static void Configure(ICommandBuilder builder)
    {
        builder.WithCommand<SourcesCommand>()
            .WithCommand<PackagesCommand>()
            .WithCommand<VulnsCommand>()
            .WithCommand<BuildsCommand>()
            .WithCommand<LicensesCommand>()
            .WithCommand<ApiKeysCommand>();
    }
}
