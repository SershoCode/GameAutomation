using GameOverlay.Drawing;

namespace GameAutomation.Core;

public class HighlightedRectangle
{
    public Guid Id { get; }
    public int Left { get; }
    public int Top { get; }
    public int Right { get; }
    public int Bottom { get; }
    public float Stroke { get; }
    public IBrush Brush { get; }

    /// <summary>
    /// Прямоугольник.
    /// </summary>
    /// <param name="left">Позиция левого верхнего угла по X.</param>
    /// <param name="top">Позиция левого верхнего угла по Y.</param>
    /// <param name="right">Позиция правого нижнего угла по X.</param>
    /// <param name="bottom">Позиция правого нижнего угла по Y.</param>
    /// <param name="stroke">Толщина.</param>
    /// <param name="brush">Кисть.</param>
    public HighlightedRectangle(int left, int top, int right, int bottom, float stroke, IBrush brush)
    {
        Id = Guid.NewGuid();
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
        Stroke = stroke;
        Brush = brush;
    }
}
