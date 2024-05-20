using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    internal sealed class HealthCommand : IConsoleCommand
    {
        public static string Name => "health";

        public static string Description => "Displays health and status information";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<SourceOption>();
        }

        public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            static bool OK(params string?[] ss) => ss.All(s => (s ?? "OK") == "OK");


            var client = context.GetProGetClient();
            Console.Write($"Checking {client.Url}...");
            var health = await client.GetInstanceHealthAsync(cancellationToken);

            var allOk = OK(health.DatabaseStatus, health.LicenseStatus, health.ServiceStatus, health.ReplicationStatus?.ServerStatus, health.ReplicationStatus?.ClientStatus);
            if (allOk)
                Console.WriteLine("all OK");
            else
                CM.WriteError("not OK");

            Console.WriteLine(
                $"""
                Version: {health.VersionNumber} ({health.VersionNumber})

                Database: {formatStatus(health.DatabaseStatus, health.DatabaseStatusDetails)}
                License:  {formatStatus(health.LicenseStatus, health.LicenseStatusDetail)}
                Service:  {formatStatus(health.ServiceStatus, health.ServiceStatusDetail)}
                """);
            if (health.ReplicationStatus is not null && (health.ReplicationStatus.ClientStatus ?? health.ReplicationStatus.ServerStatus) is not null)
            {
                Console.WriteLine(
                    $"""

                    Replication (Server): {formatStatus(health.ReplicationStatus.ServerStatus, health.ReplicationStatus.ServerError)}
                    Replication (Client): {formatStatus(health.ReplicationStatus.ClientStatus, health.ReplicationStatus.ClientError)}
                    """);
            }
            return allOk ? 0 : -1;

            static string? formatStatus(string? status, string? desc) => string.IsNullOrWhiteSpace(desc) ? status : $"{status} ({desc})";
        }
    }
}