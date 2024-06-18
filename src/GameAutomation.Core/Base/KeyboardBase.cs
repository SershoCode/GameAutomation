using System.Runtime.InteropServices;

namespace GameAutomation.Core;

public abstract partial class KeyboardBase
{
    #region WinApi

    [DllImport("user32.dll")]
    protected static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern int GetWindowThreadProcessId(int handle, out int processId);

    [DllImport("user32.dll")]
    protected static extern int GetForegroundWindow();

    [DllImport("user32.dll")]
    protected static extern int GetKeyboardLayout(int idThread);

    [DllImport("user32.dll")]
    protected static extern bool PostMessage(int hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    protected static extern IntPtr LoadKeyboardLayout(string pwszKlid, uint flags);

    #endregion

    protected const int DelayAfterAction = 15;
    protected const int DelayBeforeKeyPressed = 40;

    protected const int KeyDownEvent = 0;
    protected const int KeyUpEvent = 2;

    protected const byte ShiftKeyCode = 0x10;

    protected const int EnglishNumberLayoutCode = 67699721;
    protected const int RussianNumberLayoutCode = 68748313;

    private const string EnglishStringLayoutCode = "00000409";
    private const string RussianStringLayoutCode = "00000419";

    public virtual async Task KeyPressAsync(char keyToPress, List<VirtualSpecialKeys> specialKeys = null, bool isDetectCase = false)
    {
        var isSpecialKeysNull = specialKeys is null;

        // Получаем клавишу по ее чару.
        var key = KeyboardData.Keys.SingleOrDefault(key => key.Char == keyToPress);

        // Если клавиша не нашлась, выходим из метода.
        if (key.Char == default)
            return;

        // Проверяем, нужно ли менять раскладку и если нужно меняем.
        var currentKeyboardLayout = GetCurrentKeyboardLayout();

        if (currentKeyboardLayout != key.LayoutType && key.LayoutType != KeyboardLayout.Any)
            ChangeKeyboardLayoutByPostMessage(key.LayoutType);

        var isNeedShift = isDetectCase && key.IsUpper;

        if (isNeedShift)
        {
            if (!isSpecialKeysNull)
                specialKeys.Remove(VirtualSpecialKeys.ShiftRight);

            await KeyDownInternalAsync(ShiftKeyCode);
        }

        if (!isSpecialKeysNull)
        {
            foreach (var specialKey in specialKeys)
                await KeyDownInternalAsync((byte)specialKey);
        }

        // Нажимаем саму клавишу.
        await KeyPressInternalAsync(key.KeyCode);

        if (!isSpecialKeysNull)
        {
            foreach (var specialKey in specialKeys)
                await KeyUpInternalAsync((byte)specialKey);
        }

        if (isNeedShift)
        {
            await KeyUpInternalAsync(ShiftKeyCode);
        }
    }

    public virtual async Task PressSpecials(params VirtualSpecialKeys[] specialKeys)
    {
        foreach (var specialKey in specialKeys)
        {
            await KeyDownInternalAsync((byte)specialKey);
        }

        await Delayer.Delay(DelayBeforeKeyPressed);

        foreach (var specialKey in specialKeys)
        {
            await KeyUpInternalAsync((byte)specialKey);
        }
    }

    internal virtual async Task KeyPressInternalAsync(byte keyCode)
    {
        await KeyDownInternalAsync(keyCode);
        await KeyUpInternalAsync(keyCode);
    }

    internal virtual async Task KeyDownInternalAsync(byte keyCode)
    {
        keybd_event(keyCode, 0, KeyDownEvent, 0);

        await Delayer.Delay(DelayBeforeKeyPressed);
    }

    internal virtual async Task KeyUpInternalAsync(byte keyCode)
    {
        keybd_event(keyCode, 0, KeyUpEvent, 0);

        await Delayer.Delay(DelayBeforeKeyPressed);
    }

    public virtual async Task EnterTextAsync(string text)
    {
        foreach (var chr in text)
            await KeyPressAsync(chr, isDetectCase: true);
    }

    public virtual KeyboardLayout GetCurrentKeyboardLayout()
    {
        var currentLayout = GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), out _));

        return currentLayout switch
        {
            EnglishNumberLayoutCode => KeyboardLayout.En,
            RussianNumberLayoutCode => KeyboardLayout.Ru,
            _ => throw new NotImplementedException()
        };
    }

    public virtual void ChangeKeyboardLayoutByPostMessage(KeyboardLayout keyboardLayout)
    {
        var layoutCode = keyboardLayout switch
        {
            KeyboardLayout.En => EnglishStringLayoutCode,
            KeyboardLayout.Ru => RussianStringLayoutCode,
            _ => throw new ArgumentOutOfRangeException(nameof(keyboardLayout))
        };

       PostMessage(0xffff, 0x0050, IntPtr.Zero, LoadKeyboardLayout(layoutCode, 1));
    }
}