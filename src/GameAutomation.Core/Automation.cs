using System.Diagnostics;

namespace GameAutomation.Core;

public static class Automation
{
    private static readonly string Delimiter = new('-', 55);
    private static bool _isRunning = false;

    public static async Task LetsGo(Keys startKey, Keys stopKey, Func<Task> bottingMethod)
    {
        await ConsoleLogger.LogAsync(Delimiter);

        await ConsoleLogger.LogAsync($"Нажмите клавишу {startKey} для старта.");
        await ConsoleLogger.LogAsync($"Нажмите клавишу {stopKey} для остановки.");

        var keyboardHook = new LowLevelKeyboardHook();

        keyboardHook.OnKeyPressed += async (_, key) =>
        {
            if (key == startKey)
            {
                if(_isRunning)
                {
                    await ConsoleLogger.LogAsync("Приложение уже запущено.", ConsoleColor.Red);
                    await ConsoleLogger.LogAsync($"Если вы хотите перезапустить приложение, сначала остановите его клавишей '{stopKey}'.", ConsoleColor.Red);

                    return;
                }

                _isRunning = true;

                await bottingMethod();
            }

            if (key == stopKey)
            {
                if(!_isRunning)
                {
                    await ConsoleLogger.LogAsync("Приложение уже остановлено.", ConsoleColor.Red);
                    await ConsoleLogger.LogAsync($"Если вы хотите запустить приложение, нажмите клавишу '{startKey}'.", ConsoleColor.Red);

                    return;
                }

                _isRunning = false;

                Console.Clear();

                var currentProcess = Process.GetCurrentProcess();

                Application.Restart();

                currentProcess.Kill();
            }
        };

        keyboardHook.OnKeyUnpressed += (_, __) => { };

        keyboardHook.HookKeyboard();

        await ConsoleLogger.LogAsync(Delimiter);

        Application.Run();
    }
}
