using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal sealed partial class VulnsCommand
{
    internal sealed class PackageCommand : IConsoleCommand
    {
        public static string Name => "package";
        public static string Description => "List vulnerabilities associated with a package";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<PackageOption>()
                .WithOption<PackageTypeOption>()
                .WithOption<VersionOption>();
        }

        public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            var client = context.GetProGetClient();
            var package = context.GetOption<PackageOption>();
            var type = context.GetOption<PackageTypeOption>();
            var version = context.GetOption<VersionOption>();

            string? group = null;
            var name = package;
            int index = package.LastIndexOf('/');
            if (index >= 0)
            {
                group = package[..index];
                name = package[(index + 1)..];
            }

            return await AuditAsync(client, [new DependencyInfo(new PackageVersionIdentifier(type, name, version, group), [])], cancellationToken);
        }

        private sealed class PackageTypeOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--type";
            public static string Description => "Type of package to audit for vulnerabilities";
            public static string[] ValidValues => ["apk", "deb", "maven", "nuget", "conda", "cran", "helm", "npm", "pypi", "rpm", "gem"];
        }

        private sealed class PackageOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--package";
            public static string Description => "Name of package to audit for vulnerabilities";
        }

        private sealed class VersionOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--version";
            public static string Description => "Version of package to audit for vulnerabilities";
        }
    }
}
