using System.Text.RegularExpressions;
using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed partial class StorageCommand
        {
            private sealed partial class ChangeCommand : IConsoleCommand
            {
                public static string Name => "change";
                public static string Description => "Change feed storage configuration";
                public static bool AllowAdditionalOptions => true;
                public static string Examples => """
                      $> pgutil feeds storage change --feed=approved-nuget --type=disk --StoragePath=C:\ProgramData\Proget\Packages

                      $> pgutil feeds storage change --feed=public-pypi --type=s3 --RegionEndpoint=us-east-1 --AccessKey=EXAMPLE --SecretAccessKey=XXXXXXXXXXXXXXXXXXXX

                      $> pgutil feeds storage change --feed=approved-npm --type=azure --ConnectionString=DefaultEndpointsProtocol=https;AccountName=myazurestorage;AccountKey=XXXXXXXXXXXXXXXXXXXXX;EndpointSuffix=core.windows.net --ContainerName=projectdocuments --TargetPath=projectdocuments/uploads/2024/07/

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/feeds/proget-api-feeds/proget-api-feeds-storage-update
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<FeedOption>()
                        .WithOption<TypeOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var feed = context.GetFeedName();

                    var type = context.GetOption<TypeOption>();
                    var typeDefs = await client.ListFeedStorageTypesAsync(cancellationToken).ToListAsync();
                    var typeDef = typeDefs.FirstOrDefault(t => string.Equals(t.Id, type, StringComparison.OrdinalIgnoreCase));
                    if (typeDef is null)
                    {
                        CM.WriteError<TypeOption>($"{type} is not a valid storage type. Valid values are: {string.Join(", ", typeDefs.Select(t => t.Id))}");
                        return -1;
                    }

                    var properties = new Dictionary<string, object?>();
                    foreach (var arg in context.AdditionalOptions)
                    {
                        var m = AdditionalOptionRegex().Match(arg);
                        if (!m.Success)
                        {
                            CM.WriteError($"unexpected argument: {arg}");
                            return -1;
                        }

                        var name = m.Groups[1].Value;
                        var value = m.Groups[2].Value;
                        if (typeDef.Properties is not null && typeDef.Properties.TryGetValue(name, out var propInfo))
                        {
                            if (propInfo.Type == "boolean")
                            {
                                if (!bool.TryParse(value, out bool b))
                                {
                                    CM.WriteError($"--{name} error: expected true or false");
                                    return -1;
                                }

                                properties[name] = b;
                            }
                            else
                            {
                                properties[name] = value;
                            }
                        }
                        else
                        {
                            CM.WriteError($"unexpected argument: {arg}");
                            return -1;
                        }
                    }

                    if (typeDef.Properties is not null)
                    {
                        foreach (var prop in typeDef.Properties)
                        {
                            if (prop.Value.Required && !properties.ContainsKey(prop.Key))
                            {
                                CM.WriteError($"missing required argument: --{prop.Key}");
                                return -1;
                            }
                        }
                    }

                    await client.SetFeedStorageConfigurationAsync(
                        feed,
                        new FeedStorageConfiguration
                        {
                            Id = type,
                            Properties = properties
                        },
                        cancellationToken
                    );

                    Console.WriteLine($"Storage changed to {type}.");
                    return 0;
                }

                [GeneratedRegex("^--(?<1>[^=]+)=(?<2>.+)$")]
                private static partial Regex AdditionalOptionRegex();

                private sealed class TypeOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--type";
                    public static string Description => "Type of feed storage. Use the types command to display available options.";
                }
            }
        }
    }
}
