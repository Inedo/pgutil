using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand
    {
        private sealed partial class MetadataCommand
        {
            private sealed class GetCommand : IConsoleCommand
            {
                public static string Name => "get";
                public static string Description => "Display metadata for an asset";

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetAssetDirectoryClient();

                    var path = context.GetOption<PathOption>();
                    var info = await client.GetItemMetadataAsync(path, cancellationToken);
                    if (info is null)
                    {
                        CM.WriteError($"{path} not found.");
                        return -1;
                    }

                    Console.WriteLine($"Name: {info.Name}");
                    if (info.Size.HasValue)
                        Console.WriteLine($"Size: {info.Size}");
                    Console.Write("Type: ");
                    if (info.Directory)
                        Console.WriteLine("folder");
                    else if (!string.IsNullOrEmpty(info.Content))
                        Console.WriteLine(info.Content);
                    else
                        Console.WriteLine("auto");

                    Console.WriteLine($"Created: {info.Created.ToLocalTime()}");
                    Console.WriteLine($"Modified: {info.Modified.ToLocalTime()}");
                    Console.Write("Caching: ");
                    switch (info.CacheHeader?.Type)
                    {
                        case "NoCache":
                            Console.WriteLine("Do Not Cache");
                            break;

                        case "TTL":
                            Console.WriteLine($"TTL ({info.CacheHeader.Value} seconds)");
                            break;

                        case "Custom":
                            Console.WriteLine($"Custom ({info.CacheHeader.Value})");
                            break;

                        default:
                            Console.WriteLine("Not set (inherit)");
                            break;
                    }

                    if ((info.MD5 ?? info.SHA1 ?? info.SHA256 ?? info.SHA512) is not null)
                    {
                        Console.WriteLine();
                        Console.WriteLine("-- Hashes --");
                        if (info.MD5 is not null)
                            Console.WriteLine($"MD5: {info.MD5}");
                        if (info.SHA1 is not null)
                            Console.WriteLine($"SHA1: {info.SHA1}");
                        if (info.SHA256 is not null)
                            Console.WriteLine($"SHA256: {info.SHA256}");
                        if (info.SHA512 is not null)
                            Console.WriteLine($"SHA512: {info.SHA512}");
                    }

                    Console.WriteLine();
                    Console.WriteLine("-- Custom Metadata --");
                    if (info.UserMetadata is not null && info.UserMetadata.Count > 0)
                    {
                        foreach (var item in info.UserMetadata)
                            Console.WriteLine($"{item.Key}: {item.Value.Value}");
                    }
                    else
                    {
                        Console.WriteLine("Not set");
                    }

                    return 0;
                }
            }
        }
    }
}
