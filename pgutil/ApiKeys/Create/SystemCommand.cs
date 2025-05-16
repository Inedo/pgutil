using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    internal sealed partial class ApiKeysCommand
    {
        private sealed partial class CreateCommand
        {
            private sealed class SystemCommand : IConsoleCommand
            {
                public static string Name => "system";
                public static string Description => "Creates a system API key. The key is the only thing written to stdout on success";
                public static string Examples => """
                      $> pgutil apikeys create system --apis="feeds,sca" --expiration="2024/08/01" --description="Feeds & SCA"

                      $> pgutil apikeys create system --apis="feeds" --key=feeds12345 --logging=both --name="Feeds Key" 

                    For more information, see: https://docs.inedo.com/docs/proget/api/apikeys/create
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<KeyOption>()
                        .WithOption<NameOption>()
                        .WithOption<DescriptionOption>()
                        .WithOption<ExpirationOption>()
                        .WithOption<LoggingOption>()
                        .WithOption<ApisOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    if (!context.TryGetOption<ApisOption>(out var apiValue))
                        apiValue = "full-control";

                    var apis = apiValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    if (apis.Length == 0 
                        || apis.Contains("full-control") && apis.Length > 1
                        || apis.Any(a => a != "feeds" && a != "sca" && a != "sbom-upload" && a != "full-control"))
                    {
                        CM.WriteError<ApisOption>("an invalid value for API was specified. Expected either \"full-control\" or a comma-separated list of \"feeds\", \"sca\", and \"sbom-upload\"");
                        return -1;
                    }

                    var info = new ApiKeyInfo
                    {
                        Type = ApiKeyType.System,
                        Key = context.GetOptionOrDefault<KeyOption>(),
                        DisplayName = context.GetOptionOrDefault<NameOption>(),
                        Description = context.GetOptionOrDefault<DescriptionOption>(),
                        Expiration = context.TryGetOption<ExpirationOption, DateTime>(out var d) ? d : null,
                        Logging = context.TryGetEnumValue<LoggingOption, ApiKeyBodyLogging>(out var l) ? l : default,
                        SystemApis = apis
                    };

                    var result = await client.CreateApiKeyAsync(info, cancellationToken).ConfigureAwait(false);
                    CM.WriteLine(result.Key);
                    return 0;
                }

                private sealed class ApisOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--apis";
                    public static string Description => 
                        $"Specifies the individual APIs to give access to when creating a system API key. " +
                        $"Value is either full-control or a comma-separated list of any combination of: feeds, sca, sbom-upload";
                }

            }
        }
    }
}
