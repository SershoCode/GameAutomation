namespace GameAutomation.Core
{
    public record ObjectPosition
    {
        public int X { get; }
        public int Y { get; }
        public int Left { get; }
        public int Top { get; }
        public int Right { get; }
        public int Bottom { get; }
        public int Width { get; }
        public int Heigth { get; }

        public ObjectPosition(int x, int y, int left, int top, int right, int bottom, int width, int heigth)
        {
            X = x;
            Y = y;
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
            Width = width;
            Heigth = heigth;
        }
    }
}
