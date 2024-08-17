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
                public static string Description => "Creates a personal API key. The key is the only thing written to stdout on success";
                public static string Examples => """
                      $> pgutil apikeys create personal --username=jrdobbs --password=hunter42

                      $> pgutil apikeys create personal --user=johnsmith

                      $> pgutil apikeys create personal --user=mikejones --name="Mike Jones" --description="API key for Mike Jones"

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-apikeys/proget-api-apikeys-create
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder
                        .WithOption<ImpersonateUserOption>()
                        .WithOption<NameOption>()
                        .WithOption<DescriptionOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    var user = context.GetOptionOrDefault<ImpersonateUserOption>() ?? context.GetOptionOrDefault<UserNameOption>() ?? client.UserName;
                    if (string.IsNullOrEmpty(user))
                    {
                        CM.WriteError<ImpersonateUserOption>("required when not using --username/--password to authenticate");
                        return -1;
                    }

                    var info = new ApiKeyInfo
                    {
                        Type = ApiKeyType.Personal,
                        DisplayName = context.GetOptionOrDefault<NameOption>(),
                        Description = context.GetOptionOrDefault<DescriptionOption>(),
                        User = user,
                        Logging = ApiKeyBodyLogging.None
                    };

                    var result = await client.CreateApiKeyAsync(info, cancellationToken).ConfigureAwait(false);
                    CM.WriteLine(result.Key);
                    return 0;
                }

                private sealed class ImpersonateUserOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--user";
                    public static string Description => "Name of the user to associate with the key; required when not using --username/--password to authenticate";
                }
            }
        }
    }
}
