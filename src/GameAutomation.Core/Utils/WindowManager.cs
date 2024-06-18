using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameAutomation.Core;

public static class WindowManager
{
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rectangle rect);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out Rectangle rect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DevMode lpDevMode);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]

    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    public static WindowInfo GetWindowInfo(string windowName)
    {
        var hwnd = GetWindowHandle(windowName);

        var isWindowRectCollected = GetWindowRect(hwnd, out Rectangle windowRect);
        var isClientRectCollected = GetClientRect(hwnd, out Rectangle clientRect);

        if (isWindowRectCollected && isClientRectCollected)
        {
            return new WindowInfo()
            {
                Name = windowName,
                Left = windowRect.Left,
                Top = windowRect.Top,
                Right = windowRect.Left + clientRect.Width,
                Bottom = windowRect.Top + clientRect.Height,
                Width = clientRect.Width,
                Heigth = clientRect.Height,
            };
        }

        return null;
    }

    public static void Move(string windowName, int x, int y, int width, int height, bool isTopmost = true)
    {
        var hWnd = GetWindowHandle(windowName);

        var coordenatesRectangle = GetCoordinates(x, y, width, height);

        SetForegroundWindow(hWnd);

        MoveWindow(hWnd, coordenatesRectangle.X, coordenatesRectangle.Y, coordenatesRectangle.Width, coordenatesRectangle.Height, true);

        if(isTopmost)
            SetWindowPos(hWnd, -1, 0, 0, 0, 0, 0x0001 | 0x0002);
    }

    public static Rectangle GetCoordinates(int x, int y, int width, int heigth)
    {
        var result = new Rectangle
        {
            X = x,
            Y = y,
        };

        var screen = Screen.FromPoint(new Point(x, y));

        var devMode = new DevMode
        {
            dmSize = (short)Marshal.SizeOf(typeof(DevMode))
        };

        _ = EnumDisplaySettings(screen.DeviceName, -1, ref devMode);

        var scaling = Math.Round(decimal.Divide(devMode.dmPelsWidth, screen.Bounds.Width), 2);

        if (scaling == 1m)
        {
            result.Width = width;
            result.Height = heigth;
        }
        else
            throw new AutomationException("На данный момент приложение поддверживает только масштабирование 100%.");

        return result;
    }

    private static IntPtr GetWindowHandle(string windowName)
    {
        var process = Process.GetProcessesByName(windowName).FirstOrDefault();

        return process.MainWindowHandle;
    }
}
