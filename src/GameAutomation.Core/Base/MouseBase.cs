using System.Runtime.InteropServices;

namespace GameAutomation.Core;

public abstract partial class MouseBase()
{
    #region WinApi

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out CursorPoint p);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]

    public static partial bool SetCursorPos(int X, int Y);

    [LibraryImport("user32.dll")]
    private static partial void mouse_event(int dsFlags, int dx, int dy, int cButtons, int dsExtraInfo);

    #endregion

    public bool IsZoneEnabled { get; set; }

    protected const int DefaultDelayAfterAction = 15;

    protected const int LeftDownEvent = 0x02;
    protected const int LeftUpEvent = 0x04;

    protected const int RightDownEvent = 0x08;
    protected const int RightUpEvent = 0x10;

    protected const int MiddleDownEvent = 0x20;
    protected const int MiddleUpEvent = 0x40;

    public virtual async Task ClickLeftAsync(int x = -1, int y = -1, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, async () =>
        {
            await HoldLeftAsync();
            await ReleaseLeftAsync();
        });
    }

    public virtual async Task ClickLeftDoubleAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, async () =>
        {
            for (var counter = 0; counter < 2; counter++)
                await ClickLeftAsync();
        });
    }

    public virtual async Task ClickRightAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, async () =>
        {
            await HoldRightAsync();
            await ReleaseRightAsync();
        });
    }

    public virtual async Task ClickMiddleAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, async () =>
        {
            await HoldMiddleAsync();
            await ReleaseMiddleAsync();
        });
    }

    public virtual async Task HoldLeftAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            var (x, y) = GetCursorPosition();

            mouse_event(LeftDownEvent, x, y, 0, 0);
        });
    }

    public virtual async Task HoldRightAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            var (x, y) = GetCursorPosition();

            mouse_event(RightDownEvent, x, y, 0, 0);
        });
    }

    public virtual async Task ReleaseLeftAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            var (x, y) = GetCursorPosition();

            mouse_event(LeftUpEvent, x, y, 0, 0);
        });
    }

    public virtual async Task ReleaseRightAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            var (x, y) = GetCursorPosition();

            mouse_event(RightUpEvent, x, y, 0, 0);
        });
    }

    public virtual async Task HoldMiddleAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            var (x, y) = GetCursorPosition();

            mouse_event(MiddleDownEvent, x, y, 0, 0);
        });
    }

    public virtual async Task ReleaseMiddleAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            var (x, y) = GetCursorPosition();

            mouse_event(MiddleUpEvent, x, y, 0, 0);
        });
    }

    public virtual async Task WheelAsync(int clicks, int delayBetweenMs = DefaultDelayAfterAction)
    {
        var isWheelUp = clicks > 0;

        const int mouseWheelEvent = 0x0800;

        const int simulatedClicksForOneRealClick = 120;

        var neededSimulatedClicks = clicks * simulatedClicksForOneRealClick;

        while (isWheelUp ? neededSimulatedClicks > 0 : neededSimulatedClicks < 0)
        {
            mouse_event(mouseWheelEvent, 0, 0, isWheelUp ? simulatedClicksForOneRealClick : simulatedClicksForOneRealClick * -1, 0);

            neededSimulatedClicks = isWheelUp ? neededSimulatedClicks -= simulatedClicksForOneRealClick : neededSimulatedClicks += simulatedClicksForOneRealClick;

            await Delayer.Delay(delayBetweenMs * 7);
        }

        await Delayer.Delay(delayBetweenMs);
    }

    public virtual async Task DragAndDropAsync(int x, int y, int targetX, int targetY, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        await HoldLeftAsync(x, y, delayAfter, isSumulateMove, isRandomFinalPosition);

        await ReleaseLeftAsync(targetX, targetY, delayAfter, isSumulateMove, isRandomFinalPosition);
    }

    public virtual async Task MoveAsync(int x, int y, TimeSpan delayAfrer = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        if (isRandomFinalPosition)
        {
            const int minPixelsRandom = 1;
            const int maxPixelsRandom = 5;

            var isXUp = StaticRandom.Instance.Next(100) <= 50;
            var isYUp = StaticRandom.Instance.Next(100) <= 50;

            var randomPixelsToMoveX = StaticRandom.Instance.Next(minPixelsRandom, maxPixelsRandom);
            var randomPixelsToMoveY = StaticRandom.Instance.Next(minPixelsRandom, maxPixelsRandom);

            x = isXUp ? x + randomPixelsToMoveX : x - randomPixelsToMoveX;
            y = isYUp ? y + randomPixelsToMoveY : y - randomPixelsToMoveY;
        }

        if (!isSumulateMove)
            SetCursorPos(x, y);
        else
            await SimulateMoveAsync(x, y);

        await Delayer.Delay(delayAfrer);
    }

    private async Task SimulateMoveAsync(int x, int y)
    {
        const int mouseSpeed = 72;

        var cursorPosition = GetCursorPosition();

        var randomSpeed = Math.Max(((StaticRandom.Instance.Next(mouseSpeed) / 2.0) + mouseSpeed) / 10.0, 0.1);

        await WindMouseAsync(cursorPosition.x, cursorPosition.y, x, y, 9.0, 2.1, 10.0 / randomSpeed, 15.0 / randomSpeed, 10.0 * randomSpeed, 10.0 * randomSpeed);
    }

    private static async Task WindMouseAsync(double xs, double ys, double xe, double ye, double gravity, double wind, double minWait, double maxWait, double maxStep, double targetArea)
    {
        double sqrt2 = Math.Sqrt(2.0);
        double sqrt3 = Math.Sqrt(4.0);
        double sqrt5 = Math.Sqrt(5.0);

        double windX = 0, windY = 0, velocityX = 0, velocityY = 0;

        int newX = (int)Math.Round(xs), newY = (int)Math.Round(ys);

        var waitDiff = maxWait - minWait;

        var dist = GetHypotenuse(xe - xs, ye - ys);

        while (dist > 1.0)
        {
            wind = Math.Min(wind, dist);

            if (dist >= targetArea)
            {
                var w = StaticRandom.Instance.Next(((int)Math.Round(wind) * 2) + 1);

                windX = (windX / sqrt3) + ((w - wind) / sqrt5);
                windY = (windY / sqrt3) + ((w - wind) / sqrt5);
            }
            else
            {
                windX /= sqrt2;
                windY /= sqrt2;

                if (maxStep < 3)
                    maxStep = StaticRandom.Instance.Next(3) + 3.0;
                else
                    maxStep /= sqrt5;
            }

            velocityX += windX;
            velocityY += windY;
            velocityX += gravity * (xe - xs) / dist;
            velocityY += gravity * (ye - ys) / dist;

            if (GetHypotenuse(velocityX, velocityY) > maxStep)
            {
                var randomDist = (maxStep / 2.0) + StaticRandom.Instance.Next((int)Math.Round(maxStep) / 2);

                var velocityMag = GetHypotenuse(velocityX, velocityY);

                velocityX = (velocityX / velocityMag) * randomDist;

                velocityY = (velocityY / velocityMag) * randomDist;
            }

            var oldX = (int)Math.Round(xs);
            var oldY = (int)Math.Round(ys);

            xs += velocityX;
            ys += velocityY;

            dist = GetHypotenuse(xe - xs, ye - ys);

            newX = (int)Math.Round(xs);
            newY = (int)Math.Round(ys);

            if (oldX != newX || oldY != newY)
                SetCursorPos(newX, newY);

            var step = GetHypotenuse(xs - oldX, ys - oldY);

            var wait = (int)Math.Round((waitDiff * (step / maxStep)) + minWait);

            await Delayer.Delay(wait);
        }

        var endX = (int)Math.Round(xe);
        var endY = (int)Math.Round(ye);

        if (endX != newX || endY != newY)
            SetCursorPos(endX, endY);
    }

    private static double GetHypotenuse(double dx, double dy)
    {
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private async Task MoveAndFunc(int x, int y, TimeSpan delayAfter, bool isSumulateMove, bool isRandomFinalPosition, Func<Task> func)
    {
        if (x != 0 && y != 0)
            await MoveAsync(x, y, delayAfter, isSumulateMove, isRandomFinalPosition);

        await func();

        if(delayAfter == default)
            await Delayer.Delay(DefaultDelayAfterAction);
        else
            await Delayer.Delay(delayAfter);
    }

    private async Task MoveAndFunc(int x, int y, TimeSpan delayAfter, bool isSumulateMove, bool isRandomFinalPosition, Action action)
    {
        if (x != 0 && y != 0)
            await MoveAsync(x, y, delayAfter, isSumulateMove, isRandomFinalPosition);

        action();

        if (delayAfter == default)
            await Delayer.Delay(DefaultDelayAfterAction);
        else
            await Delayer.Delay(delayAfter);
    }

    public virtual (int x, int y) GetCursorPosition()
    {
        GetCursorPos(out var point);

        return (point.X, point.Y);
    }
}