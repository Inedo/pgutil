using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed class AttributesCommand : IConsoleCommandContainer
        {
            public static string Name => "attributes";
            public static string Description => "Views security task attributes";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithCommand<ListCommand>();
            }

            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "Lists available security task attributes";

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    await foreach (var attr in client.ListSecurityAttributesAsync(cancellationToken))
                    {
                        CM.Write(" - ", new TextSpan(attr.Name, ConsoleColor.White));
                        if (attr.FeedScoped)
                            CM.Write(ConsoleColor.Blue, "*");
                        CM.WriteLine();
                    }

                    CM.WriteLine();
                    CM.WriteLine(new TextSpan("* ", ConsoleColor.Blue), "indicates attribute can be scoped to a feed");
                    CM.WriteLine();

                    return 0;
                }
            }
        }
    }
}
