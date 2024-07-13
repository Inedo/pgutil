using ConsoleMan;

namespace PgUtil;

internal sealed class PgUtilException(string? message = null) : ConsoleManException(message)
{
}
