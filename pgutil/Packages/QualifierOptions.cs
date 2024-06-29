using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand
    {
        private static class QualifierOptions
        {
            private static readonly string[] NoQualifiers = ["helm", "npm", "nuget", "pub", "upack", "vsix"];

            public static ICommandBuilder WithOptions(ICommandBuilder builder)
            {
                return builder.WithOption<Arch>()
                    .WithOption<CondaBuild>()
                    .WithOption<CondaSubdir>()
                    .WithOption<CondaType>()
                    .WithOption<CranPath>()
                    .WithOption<CranExtension>()
                    .WithOption<DebianDistro>()
                    .WithOption<DebianComponent>()
                    .WithOption<PypiFile>()
                    .WithOption<RubyPlatform>();
            }
            public static string? GetQualifier(CommandContext context)
            {
                if (context.TryGetOption<PackageQualifierOption>(out var qualifier))
                    return qualifier;

                var values = new List<KeyValuePair<string, string>>();

                add<Arch>();
                add<CondaBuild>();
                add<CondaSubdir>();
                add<CondaType>();
                add<CranPath>();
                add<CranExtension>();
                add<DebianDistro>();
                add<DebianComponent>();
                add<PypiFile>();
                add<RubyPlatform>();

                values.Sort(compareKeys);

                return string.Join('&', values.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

                static int compareKeys(KeyValuePair<string, string> a, KeyValuePair<string, string> b) => a.Key.CompareTo(b.Key);

                void add<TOption>() where TOption : IQualifierOption
                {
                    if (context.TryGetOption<TOption>(out var value))
                        values.Add(new KeyValuePair<string, string>(TOption.Key, value));
                }
            }
            public static void EnsureQualifiersForPackageType(string packageType, string? qualifier)
            {
                var type = packageType.ToLowerInvariant();
                if (NoQualifiers.Contains(type))
                {
                    if (!string.IsNullOrEmpty(qualifier))
                        throw new PgUtilException($"Qualifiers must not be used for {type} packages.");

                    return;
                }

                switch (type)
                {
                    case "apk":
                    case "rpm":
                        ensure<Arch>();
                        break;

                    case "conda":
                        ensure<CondaSubdir>();
                        ensure<CondaBuild>();
                        ensure<CondaType>();
                        break;

                    case "cran":
                        ensure<CranPath>();
                        ensure<CranExtension>();
                        break;

                    case "deb":
                        ensure<Arch>();
                        ensure<DebianDistro>();
                        ensure<DebianComponent>();
                        break;

                    case "pypi":
                        ensure<PypiFile>();
                        break;

                    case "gem":
                        ensure<RubyPlatform>();
                        break;
                }

                void ensure<TQualifier>() where TQualifier : IQualifierOption
                {
                    if (!(qualifier ?? string.Empty).Contains($"{TQualifier.Key}="))
                    {
                        CM.WriteError<TQualifier>($"argument is required for {packageType} packages");
                        throw new PgUtilException();
                    }
                }
            }

            public sealed class Arch : IQualifierOption
            {
                public static bool Undisclosed => true;
                public static bool Required => false;
                public static string Name => "--arch";
                public static string Description => "Package architecture";
                public static string Key => "arch";
            }

            public sealed class CondaBuild : IQualifierOption
            {
                public static bool Undisclosed => true;
                public static bool Required => false;
                public static string Name => "--build";
                public static string Description => "Package build";
                public static string Key => "build";
            }

            public sealed class CondaSubdir : IQualifierOption
            {
                public static bool Undisclosed => true;
                public static bool Required => false;
                public static string Name => "--subdir";
                public static string Description => "Package subdir";
                public static string Key => "subdir";
            }

            public sealed class CondaType : IQualifierOption
            {
                public static bool Undisclosed => true;
                public static bool Required => false;
                public static string Name => "--type";
                public static string Description => "Package type";
                public static string Key => "type";
            }

            public sealed class CranPath : IQualifierOption
            {
                public static bool Undisclosed => true;
                public static bool Required => false;
                public static string Name => "--path";
                public static string Description => "Package path";
                public static string Key => "path";
            }

            public sealed class CranExtension : IQualifierOption
            {
                public static bool Undisclosed => true;
                public static bool Required => false;
                public static string Name => "--extension";
                public static string Description => "Package extension";
                public static string Key => "ext";
            }

            public sealed class DebianDistro : IQualifierOption
            {
                public static bool Undisclosed => true;
                public static bool Required => false;
                public static string Name => "--distro";
                public static string Description => "Package distribution";
                public static string Key => "distro";
            }

            public sealed class DebianComponent : IQualifierOption
            {
                public static bool Undisclosed => true;
                public static bool Required => false;
                public static string Name => "--component";
                public static string Description => "Package component";
                public static string Key => "component";
            }

            public sealed class PypiFile : IQualifierOption
            {
                public static bool Undisclosed => true;
                public static bool Required => false;
                public static string Name => "--filename";
                public static string Description => "PyPI package filename";
                public static string Key => "file";
            }

            public sealed class RubyPlatform : IQualifierOption
            {
                public static bool Undisclosed => true;
                public static bool Required => false;
                public static string Name => "--platform";
                public static string Description => "Package platform";
                public static string Key => "platform";
            }

            private interface IQualifierOption : IConsoleOption
            {
                static abstract string Key { get; }
            }
        }
    }
}
