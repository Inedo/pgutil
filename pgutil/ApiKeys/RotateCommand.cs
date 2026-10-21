using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    internal sealed partial class ApiKeysCommand
    {
        private sealed class RotateCommand : IConsoleCommand
        {
            public static string Name => "rotate";
            public static string Description => "Generate a new key for an existing API key in ProGet";
            public static string Examples => """
                  $> pgutil apikeys rotate --id=43

                For more information, see: https://docs.inedo.com/docs/proget/api/apikeys/rotate
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<IdOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                int id = context.GetOption<IdOption, int>();

                CM.WriteLine("Rotating API key ", new TextSpan($"(id={id})", ConsoleColor.White), "...");
                var newKey = await client.RotateApiKeyAsync(id, cancellationToken);
                CM.WriteLine("API key rotated. New key:");
                CM.WriteLine(ConsoleColor.Blue, newKey);
                return 0;
            }

            private sealed class IdOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--id";
                public static string Description => "ID number of the API key to rotate";
            }
        }
    }
}
