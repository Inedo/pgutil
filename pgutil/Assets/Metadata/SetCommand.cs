using ConsoleMan;
using Inedo.ProGet.AssetDirectories;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand
    {
        private sealed partial class MetadataCommand
        {
            private sealed class SetCommand : IConsoleCommandContainer
            {
                public static string Name => "set";
                public static string Description => "Sets metadata for an asset";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithCommand<CustomCommand>()
                        .WithCommand<CacheCommand>();
                }

                private sealed class CustomCommand : IConsoleCommand
                {
                    public static string Name => "custom";
                    public static string Description => "Sets a custom metadata field";
                    public static string Examples => """
                          $> pgutil assets metadata set custom --path=data-files/data.bin --feed=development-assets --key=project-name --value=project-alpha

                          $> pgutil assets metadata set cache --path=project-files/data.bin --feed=production-assets --type=TTL --value=60

                        For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-assets/metadata-endpoints/proget-api-assets-metadata-set
                        """;

                    public static void Configure(ICommandBuilder builder)
                    {
                        builder.WithOption<KeyOption>()
                            .WithOption<ValueOption>()
                            .WithOption<IncludeInHttpHeaderFlag>();
                    }

                    public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                    {
                        var client = context.GetAssetDirectoryClient();
                        var path = context.GetOption<PathOption>();
                        var key = context.GetOption<KeyOption>();
                        var value = context.GetOption<ValueOption>();

                        Console.WriteLine($"Setting {key} = {value} on {path}...");
                        await client.UpdateItemMetadataAsync(
                            path,
                            userMetadata: new Dictionary<string, AssetUserMetadata>
                            {
                                [key] = new AssetUserMetadata(value, context.HasFlag<IncludeInHttpHeaderFlag>())
                            },
                            userMetadataUpdateMode: UserMetadataUpdateMode.CreateOrUpdate,
                            cancellationToken: cancellationToken
                        );

                        Console.WriteLine("Metadata set.");
                        return 0;
                    }

                    private sealed class KeyOption : IConsoleOption
                    {
                        public static bool Required => true;
                        public static string Name => "--key";
                        public static string Description => "Key name of custom metadata field";
                    }

                    private sealed class ValueOption : IConsoleOption
                    {
                        public static bool Required => true;
                        public static string Name => "--value";
                        public static string Description => "Value of custom metadata field";
                    }

                    private sealed class IncludeInHttpHeaderFlag : IConsoleFlagOption
                    {
                        public static string Name => "--include-in-http-response";
                        public static string Description => "Include this field in HTTP responses for the asset";
                    }
                }

                private sealed class CacheCommand : IConsoleCommand
                {
                    public static string Name => "cache";
                    public static string Description => "Sets the cache header";

                    public static void Configure(ICommandBuilder builder)
                    {
                        builder.WithOption<TypeOption>()
                            .WithOption<ValueOption>();
                    }

                    public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                    {
                        var client = context.GetAssetDirectoryClient();
                        var path = context.GetOption<PathOption>();
                        var type = context.GetOption<TypeOption>();
                        var value = context.GetOptionOrDefault<ValueOption>();

                        Console.WriteLine($"Setting {path} caching to {type} {value}...");
                        await client.UpdateItemMetadataAsync(
                            path,
                            cacheHeader: new AssetDirectoryItemCacheHeader { Type = type, Value = value },
                            cancellationToken: cancellationToken
                        );

                        Console.WriteLine("Metadata set.");
                        return 0;
                    }

                    private sealed class TypeOption : IConsoleOption
                    {
                        public static bool Required => true;
                        public static string Name => "--type";
                        public static string Description => "Type of cache header";
                        public static string[] ValidValues => ["nocache", "ttl", "custom", "inherit"];
                    }

                    private sealed class ValueOption : IConsoleOption
                    {
                        public static bool Required => false;
                        public static string Name => "--value";
                        public static string Description => "Value of cache header";
                    }
                }
            }
        }
    }
}
