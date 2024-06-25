using Inedo.ProGet;

namespace Inedo.DependencyScan;

public sealed class DependencyScannerException(string? message) : ProGetClientException(message)
{
}
