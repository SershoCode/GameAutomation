namespace GameAutomation.Core;

public static class Delayer
{
    public static async Task Delay(int ms)
    {
        if (ms != 0)
            await Task.Delay(ms);
    }

    public static async Task Delay(TimeSpan timeSpan)
    {
        if (timeSpan != default)
            await Task.Delay(timeSpan);
    }
}
