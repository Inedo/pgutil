using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal sealed partial class Program : IConsoleCommandContainer
{
    public static string Name => "pgutil";
    public static string Description => "Perform operations against a ProGet server.";

    public static async Task<int> Main(string[] args)
    {
        try
        {
            return await Command.Create<Program>().ExecuteAsync(args);
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
            .WithCommand<PackagesCommand>();
    }
}
