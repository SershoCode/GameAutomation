using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameAutomation.Interception;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int Predicate(int device);

public static partial class InterceptionDriver
{
    [LibraryImport("interception.dll", EntryPoint = "interception_create_context")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static partial nint CreateContext();

    [LibraryImport("interception.dll", EntryPoint = "interception_send")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static partial int Send(nint context, int device, ref Stroke stroke, uint numStrokes);
}