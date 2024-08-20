using ConsoleMan;

namespace PgUtil;

internal sealed partial class VulnsCommand
{
    internal sealed class AssessCommand : IConsoleCommand
    {
        public static string Name => "assess";
        public static string Description => "Assess a vulnerability by specifying its ID";
        public static string Examples => """
              >$ pgutil vulns assess --id=PGV-1234567 --type=ignore

              >$ pgutil vulns assess --id=PGV-7654321 --type=caution --policy=feedPolicy

              >$ pgutil vulns assess --id=PGV-0987654 --type=blocked --comment="Package non-compliant" --policy=myPolicy

            For more information, see: https://docs.inedo.com/docs/proget/reference-api/vulnerabilities/proget-api-vulnerabilties-assess
            """;

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<VulnIdOption>()
                .WithOption<AssessmentTypeOption>()
                .WithOption<CommentOption>()
                .WithOption<PolicyOption>();
        }

        public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            var client = context.GetProGetClient();

            var id = context.GetOption<VulnIdOption>();
            var type = context.GetOption<AssessmentTypeOption>();
            _ = context.TryGetOption<CommentOption>(out var comment);
            _ = context.TryGetOption<PolicyOption>(out var policy);

            CM.Write("Assessing ", new TextSpan(id, ConsoleColor.White), " as ", new TextSpan(type, ConsoleColor.White));

            if (!string.IsNullOrWhiteSpace(policy))
                CM.Write(" with policy ", new TextSpan(policy, ConsoleColor.White));

            if (!string.IsNullOrWhiteSpace(comment))
            {
                Console.Write(" (comment: ");
                WordWrapper.WriteOutput(comment);
                Console.Write(')');
            }

            Console.WriteLine("...");

            await client.AssessVulnerabilityAsync(id, type, comment, policy, cancellationToken);

            Console.WriteLine("Assessment completed.");

            return 0;
        }

        private sealed class VulnIdOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--id";
            public static string Description => "ID of the vulnerability to assess";
        }

        private sealed class AssessmentTypeOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--type";
            public static string Description => "Assessment type in ProGet";
        }

        private sealed class CommentOption : IConsoleOption
        {
            public static bool Required => false;
            public static string Name => "--comment";
            public static string Description => "Comment to add to the vulnerability in ProGet";
        }

        private sealed class PolicyOption : IConsoleOption
        {
            public static bool Required => false;
            public static string Name => "--policy";
            public static string Description => "ProGet policy to apply to the assessment";
        }
    }
}
