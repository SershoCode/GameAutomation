using GameOverlay.Drawing;
using Font = GameOverlay.Drawing.Font;

namespace GameAutomation.Core;

public class HighlightedText
{
    public Guid Id { get; }
    public string Text { get; }
    public int Left { get; }
    public int Top { get; }
    public Font Font { get; }
    public int FontSize { get; }
    public IBrush Brush { get; }

    /// <summary>
    /// Текст.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="left">X-координата исходного положения.</param>
    /// <param name="top">Y-координата исходного положения.</param>
    /// <param name="font">Шрифт.</param>
    /// <param name="fontSize">Размер шрифта (необязательно должен быть таким же, как в Font.FontSize).</param>
    /// <param name="brush">Кисть, которая определяет цвет текста.</param>
    public HighlightedText(string text, int left, int top, Font font, int fontSize, IBrush brush)
    {
        Id = Guid.NewGuid();
        Text = text;
        Left = left;
        Top = top;
        Font = font;
        FontSize = fontSize;
        Brush = brush;
    }
}
