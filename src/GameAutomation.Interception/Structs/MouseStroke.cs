using System.Runtime.InteropServices;

namespace GameAutomation.Interception;

[StructLayout(LayoutKind.Sequential)]
public struct MouseStroke
{
    public ushort State;
    public ushort Flags;
    public short Rolling;
    public int X;
    public int Y;
    public ushort Information;
}
