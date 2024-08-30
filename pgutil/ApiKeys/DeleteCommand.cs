using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    internal sealed partial class ApiKeysCommand
    {
        private sealed class DeleteCommand : IConsoleCommand
        {
            public static string Name => "delete";
            public static string Description => "Deletes an API key from ProGet";
            public static string Examples => """
                  $> pgutil apikeys delete --id=43

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-apikeys/proget-api-apikeys-delete
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<IdOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                int id = context.GetOption<IdOption, int>();

                CM.WriteLine("Deleting API key ", new TextSpan($"(id={id})", ConsoleColor.White), "...");
                await client.DeleteApiKeyAsync(id, cancellationToken);
                CM.WriteLine("API key deleted.");
                return 0;
            }

            private sealed class IdOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--id";
                public static string Description => "ID number of the API key to delete";
            }
        }
    }
}
