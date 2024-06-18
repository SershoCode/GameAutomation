using GameOverlay.Drawing;
using GameOverlay.Windows;
using SharpDX.Direct2D1;
using Font = GameOverlay.Drawing.Font;
using Graphics = GameOverlay.Drawing.Graphics;

namespace GameAutomation.Core;

public class ScreenHighlighter
{
    public readonly GraphicsWindow _window;
    private Graphics _graphics;

    private Font _fontConsolas;
    private Font _fonrCenturyGothic;
    private Font _verdana;
    private Font _timesNewRoman;
    private Font _lucidaSansUnicode;

    private IBrush _redBrush;
    private IBrush _greenBrush;
    private IBrush _blueBrush;
    private IBrush _whiteBrush;
    private IBrush _blackBrush;
    private IBrush _purpleBrush;
    private IBrush _lightSeaGreenBrush;
    private IBrush _magneta;
    private IBrush _mediumSlateBlue;
    private IBrush _aquamarineCrayola;
    private IBrush _brilliantPurple;
    private IBrush _amethyst;
    private IBrush _heliotrope;
    private IBrush _indigo;
    private IBrush _plum;
    private IBrush _gray;

    private readonly SynchronizedCollection<HighlightedRectangle> _rectangles = [];
    private readonly SynchronizedCollection<HighlightedRectangle> _dashedRectangles = [];
    private readonly SynchronizedCollection<HighlightedText> _texts = [];
    private readonly SynchronizedCollection<HighlightedLine> _lines = [];
    private readonly SynchronizedCollection<HighlightedTriangle> _triangle = [];

    private bool isGraphicsInititalized;

    /// <summary>
    /// Позволяет рисовать фигуры и текст на экране.
    /// </summary>
    /// <param name="monitorLeft">Позиция левого верхнего угла по X. Например 0.</param>
    /// <param name="monitorTop">Позиция левого верхнего угла по Y. Например 0.</param>
    /// <param name="monitorRight">Позиция правого нижнего угла по X. Например 1920.</param>
    /// <param name="monitorBottom">Позиция правого нижнего угла по Y. Например 1080.</param>
    /// <param name="fps">FPS.</param>
    public ScreenHighlighter(int monitorLeft, int monitorTop, int monitorRight, int monitorBottom, int fps = 60)
    {
        GameOverlay.TimerService.EnableHighPrecisionTimers();

        _window = new GraphicsWindow(monitorLeft, monitorTop, monitorRight - monitorLeft, monitorBottom - monitorTop, _graphics)
        {
            FPS = fps,
            IsTopmost = true,
            IsVisible = true,
        };

        _window.DestroyGraphics += DestroyGraphics;
        _window.DrawGraphics += DrawGraphics;
        _window.SetupGraphics += SetupGraphics;

        _window.Create();
    }

    /// <summary>
    /// Метод инициализации графики.
    /// </summary>
    private void SetupGraphics(object sender, SetupGraphicsEventArgs setupGraphicsEventArgs)
    {
        _graphics = setupGraphicsEventArgs.Graphics;

        _fontConsolas = _graphics.CreateFont("Consolas", 20);
        _fonrCenturyGothic = _graphics.CreateFont("Century Gothic", 20);
        _verdana = _graphics.CreateFont("Verdana", 20);
        _timesNewRoman = _graphics.CreateFont("Times New Roman", 20);
        _lucidaSansUnicode = _graphics.CreateFont("Lucida Sans Unicode", 20);

        _redBrush = _graphics.CreateSolidBrush(255, 0, 0);
        _greenBrush = _graphics.CreateSolidBrush(0, 255, 0);
        _blueBrush = _graphics.CreateSolidBrush(0, 0, 255);
        _whiteBrush = _graphics.CreateSolidBrush(255, 255, 255);
        _blackBrush = _graphics.CreateSolidBrush(0, 0, 0);
        _purpleBrush = _graphics.CreateSolidBrush(178, 17, 225, 200);
        _lightSeaGreenBrush = _graphics.CreateSolidBrush(32, 178, 170);
        _magneta = _graphics.CreateSolidBrush(255, 0, 255);
        _mediumSlateBlue = _graphics.CreateSolidBrush(123, 104, 238);
        _aquamarineCrayola = _graphics.CreateSolidBrush(137, 205, 245);
        _brilliantPurple = _graphics.CreateSolidBrush(215, 137, 215);
        _amethyst = _graphics.CreateSolidBrush(153, 102, 204);
        _heliotrope = _graphics.CreateSolidBrush(223, 115, 255);
        _indigo = _graphics.CreateSolidBrush(89, 22, 162);
        _plum = _graphics.CreateSolidBrush(221, 160, 221);
        _gray = _graphics.CreateSolidBrush(222, 222, 222);

        _graphics.WindowHandle = _window.Handle;
        _graphics.UseMultiThreadedFactories = true;
        _graphics.VSync = true;
        _graphics.PerPrimitiveAntiAliasing = true;
        _graphics.TextAntiAliasing = true;

        isGraphicsInititalized = true;
    }

    /// <summary>
    /// Метод очистки памяти от графики.
    /// </summary>
    private void DestroyGraphics(object sender, DestroyGraphicsEventArgs destroyGraphicsEventArgs)
    {
    }

    /// <summary>
    /// Метод, который отрисовывает графику каждый кадр.
    /// </summary>
    private void DrawGraphics(object sender, DrawGraphicsEventArgs drawGraphicsEventArgs)
    {
        _graphics.ClearScene();

        foreach (var rectangle in _rectangles.ToList())
            _graphics.DrawRectangle(rectangle.Brush, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom, rectangle.Stroke);

        foreach (var dashedRectangle in _dashedRectangles.ToList())
            _graphics.DashedRectangle(dashedRectangle.Brush, dashedRectangle.Left, dashedRectangle.Top, dashedRectangle.Right, dashedRectangle.Bottom, dashedRectangle.Stroke);

        foreach (var text in _texts.ToList())
            _graphics.DrawText(text.Font, text.FontSize, text.Brush, text.Left, text.Top, text.Text);

        foreach (var line in _lines.ToList())
            _graphics.DrawLine(line.Brush, line.StartX, line.StartY, line.EndX, line.EndY, line.Stroke);

        foreach (var triangle in _triangle.ToList())
            _graphics.DrawTriangle(triangle.Brush, triangle.FirstPositionX, triangle.FirstPositionY, triangle.SecondPositionX, triangle.SecondPositionY, triangle.ThirdPositionX, triangle.ThirdPositionY, triangle.Stroke);

    }

    public void HighlightWindowInBackground(string windowName, TextPosition textPosition, TimeSpan time)
    {
        _ = Task.Run(async () =>
        {
            var info = WindowManager.GetWindowInfo(windowName);

            var (hRect, hText) = await AddRectangleWithTextAsync(info.Left,
                                                                                                info.Top,
                                                                                                info.Right,
                                                                                                info.Bottom,
                                                                                                2,
                                                                                                FigureLineType.Solid,
                                                                                                windowName,
                                                                                                FontType.CenturyGothic,
                                                                                                ColorType.Green,
                                                                                                ColorType.Green,
                                                                                                textPosition);

            await Delayer.Delay(time);

            RemoveRectangleWithText(hRect, hText);
        });
    }

    public void HighlightWindowInBackground(WindowInfo windowInfo, TimeSpan time)
    {
        _ = Task.Run(async () =>
        {
            var (hRect, hText) = await AddRectangleWithTextAsync(windowInfo.Left,
                                                                                                windowInfo.Top,
                                                                                                windowInfo.Right,
                                                                                                windowInfo.Bottom,
                                                                                                2,
                                                                                                FigureLineType.Solid,
                                                                                                windowInfo.Name,
                                                                                                FontType.CenturyGothic,
                                                                                                ColorType.Green,
                                                                                                ColorType.Green,
                                                                                                TextPosition.Top);

            await Delayer.Delay(time);

            RemoveRectangleWithText(hRect, hText);
        });
    }

    /// <summary>
    /// Нарисовать прямоугольник на экране.
    /// </summary>
    /// <param name="left">Позиция левого верхнего угла по X.</param>
    /// <param name="top">Позиция левого верхнего угла по Y.</param>
    /// <param name="right">Позиция правого нижнего угла по X.</param>
    /// <param name="bottom">Позиция правого нижнего угла по Y.</param>
    /// <param name="stroke">Толщина.</param>
    /// <param name="colorType">Цвет.</param>
    /// <param name="lineType">Тип линии.</param>
    public async Task<HighlightedRectangle> AddRectangleAsync(int left, int top, int right, int bottom, int stroke, ColorType colorType, FigureLineType lineType = FigureLineType.Solid)
    {
        await WaitForGraphicsInitialization();

        var brush = GetBrushByColor(colorType);

        var rectangle = new HighlightedRectangle(left, top, right, bottom, stroke, brush);

        if (lineType == FigureLineType.Solid)
            _rectangles.Add(rectangle);

        if (lineType == FigureLineType.Dashed)
            _dashedRectangles.Add(rectangle);

        return rectangle;
    }

    /// <summary>
    /// Текст.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="left">X-координата исходного положения.</param>
    /// <param name="top">Y-координата исходного положения.</param>
    /// <param name="fontType">Шрифт.</param>
    /// <param name="fontSize">Размер шрифта (необязательно должен быть таким же, как в Font.FontSize).</param>
    /// <param name="colorType">Цвет.</param>
    public async Task<HighlightedText> AddTextAsync(string text, int left, int top, FontType fontType, int fontSize, ColorType colorType)
    {
        await WaitForGraphicsInitialization();

        var brush = GetBrushByColor(colorType);

        var font = GetFontByFontType(fontType);

        var highlightText = new HighlightedText(text, left, top, font, fontSize, brush);

        _texts.Add(highlightText);

        return highlightText;
    }

    /// <summary>
    /// Нарисовать прямоугольник с текстом на экране.
    /// </summary>
    ///  <param name="left">Позиция левого верхнего угла по X.</param>
    /// <param name="top">Позиция левого верхнего угла по Y.</param>
    /// <param name="right">Позиция правого нижнего угла по X.</param>
    /// <param name="bottom">Позиция правого нижнего угла по Y.</param>
    /// <param name="stroke">Толщина.</param>
    /// <param name="lineType">Тип линии.</param>
    /// <param name="text">Текст.</param>
    /// <param name="fontType">Шрифт.</param>
    /// <param name="fontSize">Размер шрифта.</param>
    /// <param name="colorTypeForRectangle">Цвет для прямоугольника.</param>
    /// <param name="colorTypeForText">Цвет для текста.</param>
    /// <param name="textPosition">Позиция для текста над/под прямоугольником.</param>
    /// 
    public async Task<(HighlightedRectangle hRect, HighlightedText hText)> AddRectangleWithTextAsync(int left,
                                                                                                     int top,
                                                                                                     int right,
                                                                                                     int bottom,
                                                                                                     int stroke,
                                                                                                     FigureLineType lineType,
                                                                                                     string text,
                                                                                                     FontType fontType,
                                                                                                     ColorType colorTypeForRectangle,
                                                                                                     ColorType colorTypeForText,
                                                                                                     TextPosition textPosition,
                                                                                                     int fontSize = 24)
    {
        const int indentationForText = 10;

        var textSize = TextRenderer.MeasureText(text, new System.Drawing.Font(GetFontName(fontType), fontSize, FontStyle.Regular, GraphicsUnit.Pixel));

        var widthOfRectangle = right - left;
        var widthOfText = textSize.Width - (int)fontType;
        var HeigthOfText = textSize.Height;

        var textPositionTopBottom = textPosition == TextPosition.Top ? top - HeigthOfText - indentationForText : bottom;

        var textPositionOnLeft = widthOfRectangle > widthOfText ? left + ((widthOfRectangle - widthOfText) / 2) : left - ((widthOfText - widthOfRectangle) / 2);

        var highlightedRectangle = await AddRectangleAsync(left, top, right, bottom, stroke, colorTypeForRectangle, lineType);

        var highlightedText = await AddTextAsync(text, textPositionOnLeft, textPositionTopBottom, fontType, fontSize, colorTypeForText);

        return (highlightedRectangle, highlightedText);
    }

    public async Task<HighlightedLine> AddLineAsync(int startX, int startY, int endX, int endY, ColorType colorType, float stroke = 3)
    {
        await WaitForGraphicsInitialization();

        var brush = GetBrushByColor(colorType);

        var line = new HighlightedLine(startX, startY, endX, endY, stroke, brush);

        _lines.Add(line);

        return line;
    }

    public void RemoveRectangle(HighlightedRectangle rectangle)
    {
        _rectangles.Remove(rectangle);
    }

    public void RemoveText(HighlightedText text)
    {
        _texts.Remove(text);
    }

    public void RemoveRectangleWithText(HighlightedRectangle rectangle, HighlightedText text)
    {
        _rectangles.Remove(rectangle);
        _texts.Remove(text);
    }

    public void HighlightObjectByRectangle(ObjectPosition objectPosition, TimeSpan time, int stroke = 1, ColorType color = ColorType.Green, FigureLineType lineType = FigureLineType.Solid)
    {
        if (objectPosition == null)
            return;

        _ = Task.Run(async () =>
        {
            var rectangle = await AddRectangleAsync(objectPosition.Left - stroke, objectPosition.Top - stroke, objectPosition.Right + stroke, objectPosition.Bottom + stroke, stroke, color, lineType);

            await Delayer.Delay(time);

            RemoveRectangle(rectangle);
        });
    }

    public void HighlightObjectByRectangleWithText(ObjectPosition objectPosition,
                                                   string text,
                                                   TimeSpan time,
                                                   int stroke = 1,
                                                   ColorType rectangleColor = ColorType.Green,
                                                   FigureLineType lineType = FigureLineType.Solid,
                                                   FontType fontType = FontType.CenturyGothic,
                                                   ColorType fontColor = ColorType.Green,
                                                   TextPosition textPosition = TextPosition.Top)
    {
        if (objectPosition == null)
            return;

        _ = Task.Run(async () =>
        {
            var (hRect, hText) = await AddRectangleWithTextAsync(objectPosition.Left - stroke,
                                                                                                    objectPosition.Top - stroke,
                                                                                                    objectPosition.Right + stroke,
                                                                                                    objectPosition.Bottom + stroke,
                                                                                                    stroke,
                                                                                                    lineType,
                                                                                                    text,
                                                                                                    fontType,
                                                                                                    rectangleColor,
                                                                                                    fontColor,
                                                                                                    textPosition);


            await Delayer.Delay(time);

            RemoveRectangleWithText(hRect, hText);
        });
    }

    private IBrush GetBrushByColor(ColorType colorType)
    {
        // Цвет сначала в сам Enum, а потом в этом методе по его значению отдаем нужную кисть.
        return colorType switch
        {
            ColorType.Red => _redBrush,
            ColorType.Green => _greenBrush,
            ColorType.Blue => _blueBrush,
            ColorType.White => _whiteBrush,
            ColorType.Black => _blackBrush,
            ColorType.Puple => _purpleBrush,
            ColorType.LightSeaGreen => _lightSeaGreenBrush,
            ColorType.Magneta => _magneta,
            ColorType.MediumSlateBlue => _mediumSlateBlue,
            ColorType.AquamarineCrayola => _aquamarineCrayola,
            ColorType.BrilliantPurple => _brilliantPurple,
            ColorType.Amethyst => _amethyst,
            ColorType.Heliotrope => _heliotrope,
            ColorType.Indigo => _indigo,
            ColorType.Plum => _plum,
            ColorType.Gray => _gray,

            _ => throw new NotImplementedException(),
        };
    }

    private Font GetFontByFontType(FontType fontType)
    {
        return fontType switch
        {
            FontType.Consolas => _fontConsolas,
            FontType.CenturyGothic => _fonrCenturyGothic,
            FontType.Verdana => _verdana,
            FontType.TimesNewRoman => _timesNewRoman,
            FontType.LucidaSansUnicode => _lucidaSansUnicode,
            _ => throw new NotImplementedException(),
        };
    }

    private string GetFontName(FontType fontType)
    {
        return fontType switch
        {
            FontType.Consolas => "Consolas",
            FontType.CenturyGothic => "Century Gothic",
            FontType.Verdana => "Verdana",
            FontType.TimesNewRoman => "Times New Roman",
            FontType.LucidaSansUnicode => "Lucida Sans Unicode",
            _ => throw new NotImplementedException(),
        };
    }

    private async Task WaitForGraphicsInitialization()
    {
        while (!isGraphicsInititalized)
        {
            await Task.Delay(50);
        }
    }
}