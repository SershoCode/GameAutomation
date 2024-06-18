using GameAutomation.Interception;

namespace GameAutomation.Core;

public class DriverKeyboard(int deviceId = 1) : KeyboardBase
{
    private readonly nint context = InterceptionDriver.CreateContext();
    private readonly int _deviceId = deviceId;
    private Stroke _keyboardStroke = new() { Key = new KeyStroke() };

    private new const ushort KeyDownEvent = 0x00;
    private new const ushort KeyUpEvent = 0x01;

    private new const ushort ShiftKeyCode = 42;

    public async Task KeyPressAsync(char keyToPress, List<DriverSpecialKeys> specialKeys = null, bool isDetectCase = false)
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
            await ChangeKeyboardLayoutByAltShift(key.LayoutType);

        // Проверяем нужно ли зажимать модификаторы для ввода символа.
        var isNeedShift = isDetectCase && key.IsUpper;

        if (isNeedShift)
        {
            if (!isSpecialKeysNull)
                specialKeys.Remove(DriverSpecialKeys.ShiftRight);

            await KeyDownInternalAsync(ShiftKeyCode);
        }

        if (!isSpecialKeysNull)
        {
            foreach (var specialKey in specialKeys)
                await KeyDownInternalAsync((ushort)specialKey);
        }

        // Нажимаем саму клавишу.
        await KeyPressInternalAsync(key.DriverCode);

        if (!isSpecialKeysNull)
        {
            foreach (var specialKey in specialKeys)
                await KeyUpInternalAsync((ushort)specialKey);
        }

        if (isNeedShift)
            await KeyUpInternalAsync(ShiftKeyCode);
    }

    public override async Task EnterTextAsync(string text)
    {
        foreach (var chr in text)
            await KeyPressAsync(chr, isDetectCase: true);
    }

    public async Task PressSpecials(params DriverSpecialKeys[] specialKeys)
    {
        foreach (var specialKey in specialKeys)
        {
            await KeyDownInternalAsync((ushort)specialKey);
        }

        await Delayer.Delay(DelayBeforeKeyPressed);

        foreach (var specialKey in specialKeys)
        {
            await KeyUpInternalAsync((ushort)specialKey);
        }
    }

    public async Task KeyPressAsync(DriverSpecialKeys keyToPress)
    {
        await KeyPressInternalAsync((ushort)keyToPress);
    }

    public async Task KeyPressInternalAsync(ushort keyCode)
    {
        await KeyDownInternalAsync(keyCode);
        await KeyUpInternalAsync(keyCode);
    }

    private async Task KeyDownInternalAsync(ushort keyCode)
    {
        _keyboardStroke.Key.Code = keyCode;
        _keyboardStroke.Key.State = KeyDownEvent;

        _ = InterceptionDriver.Send(context, _deviceId, ref _keyboardStroke, 1);

        await Delayer.Delay(DelayBeforeKeyPressed);
    }

    private async Task KeyUpInternalAsync(ushort keyCode)
    {
        _keyboardStroke.Key.Code = keyCode;
        _keyboardStroke.Key.State = KeyUpEvent;

        _ = InterceptionDriver.Send(context, _deviceId, ref _keyboardStroke, 1);

        await Delayer.Delay(DelayBeforeKeyPressed);
    }

    public async Task ChangeKeyboardLayoutByAltShift(KeyboardLayout targetLayout)
    {
        var currentLayout = GetCurrentKeyboardLayout();

        if (currentLayout == targetLayout)
            return;

        // Переключаем раскладку.
        await PressSpecials(DriverSpecialKeys.AltRight, DriverSpecialKeys.ShiftRight);
    }
}