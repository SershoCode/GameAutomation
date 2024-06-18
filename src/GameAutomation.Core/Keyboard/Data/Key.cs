namespace GameAutomation.Core;

public struct Key
{
    public char Char;
    public byte KeyCode;
    public ushort DriverCode;
    public bool IsUpper;
    public KeyboardLayout LayoutType;

    public Key(char @char, byte keyCode, ushort driverCode, bool isUpper, KeyboardLayout layoutType)
    {
        Char = @char;
        KeyCode = keyCode;
        DriverCode = driverCode;
        IsUpper = isUpper;
        LayoutType = layoutType;
    }
}
