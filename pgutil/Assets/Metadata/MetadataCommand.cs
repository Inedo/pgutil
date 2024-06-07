using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand
    {
        private sealed partial class MetadataCommand : IConsoleCommandContainer
        {
            public static string Name => "metadata";
            public static string Description => "View or modify asset metadata";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PathOption>()
                    .WithCommand<GetCommand>()
                    .WithCommand<SetCommand>()
                    .WithCommand<DeleteCommand>();
            }

            private sealed class PathOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--path";
                public static string Description => "Path of item to inspect";
            }
       }
    }
}
