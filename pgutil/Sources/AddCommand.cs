using ConsoleMan;
using PgUtil.Config;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SourcesCommand
    {
        private sealed class AddCommand : IConsoleCommand
        {
            public static string Name => "add";
            public static string Description => "Adds a source to the configuration file";
            public static string Examples => """
                  $> pgutil sources add --name=Default --url=https://proget.corp.local/ --api-key=abc12345
                  $> pgutil sources add --name=main --url=https://inedo.corp.test/ --api-key=xyz7890abc

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-pgutil#working-with-sources
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<NameOption>()
                    .WithOption<UrlOption>()
                    .WithOption<FeedOption>()
                    .WithOption<ApiKeyOption>()
                    .WithOption<UserNameOption>()
                    .WithOption<PasswordOption>()
                    .WithOption<PlainTextFlag>();
            }

            public static Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var name = context.GetOption<NameOption>();
                if (string.IsNullOrWhiteSpace(name) || name.Trim() != name)
                {
                    CM.WriteError<NameOption>("Name cannot start or end with whitespace.");
                    return Task.FromResult(-1);
                }

                var url = context.GetOption<UrlOption>();
                if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                {
                    CM.WriteError<UrlOption>("Invalid url.");
                    return Task.FromResult(-1);
                }

                if (context.TryGetOption<FeedOption>(out var feed) && (string.IsNullOrWhiteSpace(feed) || feed.Trim() != feed))
                {
                    CM.WriteError<FeedOption>("Feed cannot start or end with whitespace.");
                    return Task.FromResult(-1);
                }

                var sources = PgUtilConfig.Instance.Sources.ToList();
                sources.RemoveAll(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                var newSource = new PgUtilSource(
                    name,
                    url,
                    feed,
                    context.GetOptionOrDefault<UserNameOption>(),
                    context.GetOptionOrDefault<PasswordOption>(),
                    context.GetOptionOrDefault<ApiKeyOption>()
                );

                bool hasCreds = newSource.Password is not null || newSource.Token is not null;

                if (hasCreds)
                {
                    if (!context.HasFlag<PlainTextFlag>())
                    {
                        newSource = newSource.Obfuscate();
                        CM.WriteLine(ConsoleColor.Yellow, "Storing credentials. Note: Sensitive values are obfuscated but not securely encrypted.");
                    }
                    else
                    {
                        CM.WriteLine(ConsoleColor.Yellow, "Storing credentials in plain text.");
                    }
                }

                sources.Add(newSource);

                var config = PgUtilConfig.Instance with { Sources = [.. sources] };
                config.Save(PgUtilConfig.ConfigFilePath);

                return Task.FromResult(0);
            }

            private sealed class NameOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--name";
                public static string Description => "Name of the source. Must be unique.";
                public static string DefaultValue => "Default";
            }

            private sealed class UrlOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--url";
                public static string Description => "ProGet source URL";
            }

            private sealed class FeedOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--feed";
                public static string Description => "Default feed for feed-specific operations";
            }

            private sealed class UserNameOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--username";
                public static string Description => "ProGet user name to use for authentication";
            }

            private sealed class PasswordOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--password";
                public static string Description => "ProGet password to use for authentication";
            }

            private sealed class PlainTextFlag : IConsoleFlagOption
            {
                public static string Name => "--plain-text";
                public static string Description => "Store password or API key as plain text instead of obfuscated";
            }
        }
    }
}
