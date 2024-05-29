namespace RevitPlugin.Model
{
    public struct RoomFormat
    {
        public readonly double MinWidth;
        public readonly double MaxWidth;
        public readonly double MinSquare;
        public readonly double MaxSquare;
        public readonly double RecWidth;

        public RoomFormat(double minWidth = 0, double maxWidth = double.MaxValue, double recWidth = -1,
            double minSquare = 0, double maxSquare = double.MaxValue)
        {
            MinWidth = minWidth;
            MaxWidth = maxWidth;
            MinSquare = minSquare;
            MaxSquare = maxSquare;
            RecWidth = recWidth;
        }

        public static RoomFormat operator *(RoomFormat roomFormat, double number)
        {
            return new RoomFormat(roomFormat.MinWidth * number,
                roomFormat.MaxWidth * number,
                roomFormat.RecWidth * number,
                roomFormat.MinSquare * number,
                roomFormat.MaxSquare * number
            );
        }
    }
}