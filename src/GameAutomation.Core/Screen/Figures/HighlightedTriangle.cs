using GameOverlay.Drawing;

namespace GameAutomation.Core;

public class HighlightedTriangle
{
    public float FirstPositionX { get; }
    public float FirstPositionY { get; }
    public float SecondPositionX { get; }
    public float SecondPositionY { get; }
    public float ThirdPositionX { get; }
    public float ThirdPositionY { get; }
    public float Stroke { get; }
    public IBrush Brush { get; }


    public HighlightedTriangle(float firstPositionX,
                               float firstPositionY,
                               float secondPositionX,
                               float secondPositionY,
                               float thirdPositionX,
                               float thirdPositionY,
                               float stroke,
                               IBrush brush)
    {
        FirstPositionX = firstPositionX;
        FirstPositionY = firstPositionY;
        SecondPositionX = secondPositionX;
        SecondPositionY = secondPositionY;
        ThirdPositionX = thirdPositionX;
        ThirdPositionY = thirdPositionY;
        Stroke = stroke;
        Brush = brush;
    }
}

