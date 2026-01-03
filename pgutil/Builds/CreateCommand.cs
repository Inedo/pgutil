using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed class CreateCommand : IConsoleCommand
        {
            public static string Name => "create";
            public static string Description => "Creates or updates a build";
            public static string Examples => """
                >$ pgutil builds create --build=1.0.0 --project=testApplication

                For more information, see: https://docs.inedo.com/docs/proget/api/sca/builds/create
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithProGetClientOptions()
                    .WithOption<ProjectOption>()
                    .WithOption<BuildOption>()
                    .WithOption<InactiveFlag>()
                    .WithOption<StageOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                bool? active = null;
                if (context.TryGetEnumValue<StatusOption, BuildStatus>(out var status))
                    active = status == BuildStatus.Active;
                else if (context.HasFlag<InactiveFlag>())
                    active = false;

                    _ = await client.CreateOrUpdateBuildAsync(
                        new Inedo.ProGet.CreateOrUpdateBuildOptions
                        {
                            Project = context.GetOption<ProjectOption>(),
                            Version = context.GetOption<BuildOption>(),
                            Active = active,
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
                public static bool Undisclosed => true;
            }

            private sealed class StatusOption : IConsoleEnumOption<BuildStatus>
            {
                public static bool Required => false;
                public static string Name => "--status";
                public static string Description => "Sets the status of the build";
            }

            private sealed class StageOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--stage";
                public static string Description => "Initial pipeline stage of the build";
            }

            private enum BuildStatus
            {
                Active,
                Archived
            }
        }
    }
}
