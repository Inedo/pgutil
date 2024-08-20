using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed partial class DetectionCommand : IConsoleCommandContainer
        {
            public static string Name => "detection";
            public static string Description => "Adds or removes license detection options";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<CodeOption>()
                    .WithOption<TypeOption>()
                    .WithOption<ValueOption>()
                    .WithCommand<AddCommand>()
                    .WithCommand<RemoveCommand>();
            }

            private sealed class TypeOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--type";
                public static string Description => "License detection type";
                public static string[] ValidValues => ["spdx", "url", "packagename", "purl"];
            }

            private sealed class ValueOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--value";
                public static string Description => "Detection value for the specified detection type";
            }
        }
    }
}
