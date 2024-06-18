using GameAutomation.Interception;

namespace GameAutomation.Core;

public class DriverMouse(int deviceId = 12) : MouseBase
{
    private readonly nint context = InterceptionDriver.CreateContext();
    private readonly int _deviceId = deviceId;
    private Stroke _mouseStroke = new() { Mouse = new MouseStroke() };

    private new const ushort LeftDownEvent = 0x01;
    private new const ushort LeftUpEvent = 0x02;

    private new const ushort RightDownEvent = 0x04;
    private new const ushort RightUpEvent = 0x08;

    private new const ushort MiddleDownEvent = 0x10;
    private new const ushort MiddleUpEvent = 0x20;

    private const ushort ScrollEvent = 0x400;
    private const ushort MoveRelativeEvent = 0x000;

    public override async Task HoldLeftAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        _mouseStroke.Mouse.State = LeftDownEvent;

        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            _ = InterceptionDriver.Send(context, _deviceId, ref _mouseStroke, 1);
        });
    }

    public override async Task HoldRightAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        _mouseStroke.Mouse.State = RightDownEvent;

        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            _ = InterceptionDriver.Send(context, _deviceId, ref _mouseStroke, 1);
        });
    }

    public override async Task ReleaseLeftAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        _mouseStroke.Mouse.State = LeftUpEvent;

        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            _ = InterceptionDriver.Send(context, _deviceId, ref _mouseStroke, 1);
        });
    }

    public override async Task ReleaseRightAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        _mouseStroke.Mouse.State = RightUpEvent;

        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            _ = InterceptionDriver.Send(context, _deviceId, ref _mouseStroke, 1);
        });
    }

    public override async Task HoldMiddleAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        _mouseStroke.Mouse.State = MiddleDownEvent;

        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            var (x, y) = GetCursorPosition();

            _ = InterceptionDriver.Send(context, _deviceId, ref _mouseStroke, 1);
        });
    }

    public override async Task ReleaseMiddleAsync(int x = 0, int y = 0, TimeSpan delayAfter = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
    {
        _mouseStroke.Mouse.State = MiddleUpEvent;

        await MoveAndFunc(x, y, delayAfter, isSumulateMove, isRandomFinalPosition, () =>
        {
            _ = InterceptionDriver.Send(context, _deviceId, ref _mouseStroke, 1);
        });
    }

    public override async Task WheelAsync(int clicks, int delayBetweenMs = DefaultDelayAfterAction)
    {
        var isWheelUp = clicks > 0;

        _mouseStroke.Mouse.State = ScrollEvent;
        _mouseStroke.Mouse.Rolling = (short)(isWheelUp ? 120 : -120);

        while (isWheelUp ? clicks > 0 : clicks < 0)
        {
            _ = InterceptionDriver.Send(context, 12, ref _mouseStroke, 1);

            clicks = clicks > 0 ? --clicks : ++clicks;

            await Delayer.Delay(delayBetweenMs * 7);
        }

        await Delayer.Delay(delayBetweenMs);
    }

    public override async Task MoveAsync(int x, int y, TimeSpan delayAfrer = default, bool isSumulateMove = true, bool isRandomFinalPosition = true)
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
            MoveTo(x, y);
        else
            await SimulateMoveAsync(x, y);

        await Delayer.Delay(delayAfrer);
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

    private async Task SimulateMoveAsync(int x, int y)
    {
        const int mouseSpeed = 65;

        var cursorPosition = GetCursorPosition();

        var randomSpeed = Math.Max(((StaticRandom.Instance.Next(mouseSpeed) / 2.0) + mouseSpeed) / 10.0, 0.1);

        await WindMouseAsync(cursorPosition.x, cursorPosition.y, x, y, 9.0, 2.1, 10.0 / randomSpeed, 15.0 / randomSpeed, 10.0 * randomSpeed, 10.0 * randomSpeed);
    }

    private async Task WindMouseAsync(double xs, double ys, double xe, double ye, double gravity, double wind, double minWait, double maxWait, double maxStep, double targetArea)
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
                MoveTo(newX, newY);

            var step = GetHypotenuse(xs - oldX, ys - oldY);

            var wait = (int)Math.Round((waitDiff * (step / maxStep)) + minWait);

            await Delayer.Delay(wait);
        }

        var endX = (int)Math.Round(xe);
        var endY = (int)Math.Round(ye);

        if (endX != newX || endY != newY)
            MoveTo(endX, endY);
    }

    private static double GetHypotenuse(double dx, double dy)
    {
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private void MoveTo(int x, int y)
    {
        var currentPos = GetCursorPosition();

        var xMove = x - currentPos.x;
        var yMove = y - currentPos.y;

        _mouseStroke.Mouse.X = xMove;
        _mouseStroke.Mouse.Y = yMove;

        _mouseStroke.Mouse.Flags = MoveRelativeEvent;

        InterceptionDriver.Send(context, 12, ref _mouseStroke, 1);
    }
}