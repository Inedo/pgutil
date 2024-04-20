using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    internal sealed partial class ApiKeysCommand
    {
        private sealed partial class CreateCommand
        {
            private sealed class PersonalCommand : IConsoleCommand
            {
                public static string Name => "personal";
                public static string Description => "Creates a personal API key.  The key is the only thing written to stdout on success";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<NameOption>()
                        .WithOption<DescriptionOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    var info = new ApiKeyInfo
                    {
                        Type = ApiKeyType.Personal,
                        DisplayName = context.GetOptionOrDefault<NameOption>(),
                        Description = context.GetOptionOrDefault<DescriptionOption>(),
                    };

                    var result = await client.CreateApiKeyAsync(info, cancellationToken).ConfigureAwait(false);
                    CM.WriteLine(result.Key);
                    return 0;
                }
            }
        }
    }
}
