namespace GameAutomation.Core;

public class AverageColor
{
    public int Red { get; }
    public int Green { get; }
    public int Blue { get; }

    // Вот тут переделываем под int[].
    public string SumString { get; }

    public ColorCode PriorityColor { get; }

    public AverageColor(int red, int green, int blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
        SumString = $"{red} {green} {blue}";
        PriorityColor = GetPriorityColor();
    }

    private ColorCode GetPriorityColor()
    {
        if (Red >= Green && Red >= Blue)
            return ColorCode.Red;

        if (Green >= Red && Green >= Blue)
            return ColorCode.Red;

        if (Blue >= Red && Blue >= Green)
            return ColorCode.Red;

        return ColorCode.None;
    }
}