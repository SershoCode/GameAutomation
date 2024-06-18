using System.Runtime.InteropServices;

namespace GameAutomation.Interception;

[StructLayout(LayoutKind.Sequential)]
public struct KeyStroke
{
    public ushort Code;
    public ushort State;
    public uint Information;
}
