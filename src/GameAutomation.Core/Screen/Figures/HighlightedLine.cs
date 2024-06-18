using GameOverlay.Drawing;

namespace GameAutomation.Core;

public class HighlightedLine
{
    public int StartX { get; }
    public int StartY { get; }
    public int EndX { get; }
    public int EndY { get; }
    public float Stroke { get; }
    public IBrush Brush { get; }

    public HighlightedLine(int startX, int startY, int endX, int endY, float stroke, IBrush brush)
    {
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
        Stroke = stroke;
        Brush = brush;
    }
}
