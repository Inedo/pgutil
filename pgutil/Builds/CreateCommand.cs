using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed class CreateCommand : IConsoleCommand
        {
            public static string Name => "create";
            public static string Description => "Creates a build";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<SourceOption>()
                    .WithOption<ApiKeyOption>()
                    .WithOption<UserNameOption>()
                    .WithOption<PasswordOption>()
                    .WithOption<ProjectOption>()
                    .WithOption<BuildOption>()
                    .WithOption<InactiveFlag>()
                    .WithOption<StageOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                _ = await client.CreateOrUpdateBuildAsync(
                    new Inedo.ProGet.CreateOrUpdateBuildOptions
                    {
                        Project = context.GetOption<ProjectOption>(),
                        Version = context.GetOption<BuildOption>(),
                        Active = context.HasFlag<InactiveFlag>() ? false : null,
                        Stage = context.GetOptionOrDefault<StageOption>()
                    },
                    cancellationToken
                );

                Console.WriteLine("Build created.");
                return 0;
            }

            private sealed class InactiveFlag : IConsoleFlagOption
            {
                public static string Name => "--inactive";
                public static string Description => "Create the build in an inactive state";
            }

            private sealed class StageOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--stage";
                public static string Description => "Initial pipeline stage of the build";
            }
        }
    }
}
