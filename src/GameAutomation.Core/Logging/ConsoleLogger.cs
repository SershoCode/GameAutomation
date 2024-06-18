namespace GameAutomation.Core;

public static class ConsoleLogger
{
    private const ConsoleColor DefaultTextColor = ConsoleColor.White;

    public static async Task LogAsync(string message, ConsoleColor color = DefaultTextColor)
    {
        if (color != DefaultTextColor)
            Console.ForegroundColor = color;

        await Console.Out.WriteLineAsync($"[{DateTime.Now:HH:mm:ss}] {message}");

        if(color != DefaultTextColor)
            Console.ForegroundColor = DefaultTextColor;
    }
}