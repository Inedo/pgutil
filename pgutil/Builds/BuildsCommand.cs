﻿using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand : IConsoleCommandContainer
    {
        public static string Name => "builds";
        public static string Description => "Manage SCA builds and SBOM documents";
        public static string Examples => """
              >$ pgutil builds create --build=1.0.0 --project=testApplication

            For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-sca/builds/proget-api-sca-builds-create
            """;

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithCommand<CreateCommand>()
                .WithCommand<InfoCommand>()
                .WithCommand<ListCommand>()
                .WithCommand<AuditCommand>()
                .WithCommand<PromoteCommand>()
                .WithCommand<ScanCommand>()
                .WithCommand<ProjectsCommand>()
                .WithCommand<IssuesCommand>()
                .WithCommand<CommentsCommand>()
                .WithCommand<SbomCommand>();
        }

        private sealed class ProjectOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--project";
            public static string Description => "Name of the project";
        }

        private sealed class BuildOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--build";
            public static string Description => "Build number";
        }

        private sealed class InputOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--input";
            public static string Description => "Project to scan for dependencies";
        }

        private sealed class ProjectNameOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--project-name";
            public static string Description => "Name of the component consuming the dependencies";
        }

        private sealed class VersionOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--version";
            public static string Description => "Version of the component consuming the dependencies";
        }

        private sealed class ProjectTypeOption : IConsoleOption
        {
            public static bool Required => false;
            public static string Name => "--project-type";
            public static string Description => "Type of the consuming project (default=library)";
            public static string DefaultValue => "library";
        }

        private sealed class IncludeProjectReferencesFlag : IConsoleFlagOption
        {
            public static string Name => "--include-project-references";
            public static string Description => "Include dependencies from referenced projects in the generated SBOM document";
        }
    }
}
