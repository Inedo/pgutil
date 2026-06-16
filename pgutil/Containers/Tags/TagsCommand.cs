using ConsoleMan;

namespace PgUtil;

partial class Program
{
    partial class ContainersCommand
    {
        private sealed partial class TagsCommand : IConsoleCommandContainer
        {
            public static string Name => "tags";
            public static string Description => "Add or delete tags on container images in ProGet";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithCommand<AddCommand>()
                    .WithCommand<DeleteCommand>();
            }
        }
    }
}
