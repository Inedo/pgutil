using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    internal sealed partial class ApiKeysCommand
    {
        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "Lists API keys";

            public static void Configure(ICommandBuilder builder)
            {
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                int count = 0;
                await foreach (var key in client.ListApiKeysAsync(cancellationToken))
                {
                    count++;
                    CM.WriteLine(key.DisplayName ?? "(unnamed key)");
                    
                    var data = new List<(string, string)>
                    {
                        ("  Id:", key.Id.ToString()!),
                        ("  Expiration:", key.Expiration?.ToShortDateString() ?? "None"),
                        ("  Logging:", key.Logging.ToString()),
                        ("  Type:", (key.Type).ToString())
                    };
                    if (key.Type == ApiKeyType.System)
                        data.Add(("  APIs:", string.Join(", ", key.SystemApis ?? [])));
                    else if (key.Type == ApiKeyType.Personal)
                        data.Add(("  User:", key.User ?? "(unknown)"));
                    else if (key.Type == ApiKeyType.Feed)
                    {
                        data.Add(key.FeedGroup is not null ? ("  Feed group:", key.FeedGroup) : ("  Feed:", key.Feed ?? "(unknown)"));
                        data.Add(("  Permissions:", string.Join(", ", key.PackagePermissions ?? [])));
                    }
                    CM.WriteTwoColumnList(data);
                    CM.WriteLine();
                }

                if (count == 0)
                    CM.WriteLine("No API keys are defined.");

                return 0;
            }
        }
    }
}
