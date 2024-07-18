using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed partial class StorageCommand
        {
            private sealed class TypesCommand : IConsoleCommand
            {
                public static string Name => "types";
                public static string Description => "List storage types supported on this source";

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    await foreach (var fs in client.ListFeedStorageTypesAsync(cancellationToken))
                    {
                        CM.WriteLine("Type: ", new TextSpan(fs.Id, ConsoleColor.White));
                        CM.WriteLine(fs.Description);
                        CM.WriteLine("Options:");
                        if (fs.Properties is not null)
                        {
                            foreach (var p in fs.Properties)
                            {
                                CM.Write(new TextSpan($"  --{p.Key}", ConsoleColor.White), "=<value>");
                                if (p.Value.Required)
                                    CM.WriteLine(ConsoleColor.Blue, " *REQUIRED* ");
                                else
                                    CM.WriteLine();

                                if (!string.IsNullOrEmpty(p.Value.Description))
                                {
                                    Console.Write("    ");
                                    WordWrapper.WriteOutput(p.Value.Description, 4);
                                    Console.WriteLine();
                                }

                                if (p.Value.Type == "boolean")
                                    CM.WriteLine("    Must be true or false (defaults to false).");

                                if (!string.IsNullOrEmpty(p.Value.Placeholder))
                                    CM.WriteLine($"    Default: {p.Value.Placeholder}");
                            }
                        }

                        CM.WriteLine();
                    }

                    return 0;
                }
            }
        }
    }
}
