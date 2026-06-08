using Inedo.ProGet;

namespace PgUtil;

internal static class ConsoleFormatExtensions
{
    extension(VulnerabilityInfo vulnerability)
    {
        public ConsoleColor SeverityColor => vulnerability.NumericCvss switch
        {
            null => default,
            >= 7 => ConsoleColor.Red,
            >= 4 => ConsoleColor.DarkYellow,
            _ => ConsoleColor.Yellow
        };

        public ConsoleColor? AssessmentColor => vulnerability.Assessment switch
        {
            "Contain" => ConsoleColor.Red,
            "Remediate" => ConsoleColor.DarkYellow,
            "Monitor" => ConsoleColor.Green,
            _ => null
        };

        public ConsoleColor CategoryColor => vulnerability.Pvrs switch
        {
            5 => ConsoleColor.Red,
            4 => ConsoleColor.Red,
            3 => ConsoleColor.DarkYellow,
            2 => ConsoleColor.Green,
            //1 is actually FAFAFA,but that color blends with the background
            1 => ConsoleColor.Green,
            _ => ConsoleColor.Green
        };
    }
}
