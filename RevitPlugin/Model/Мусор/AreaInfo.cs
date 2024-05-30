using System.Collections.Generic;

namespace AreaRoomsAPI.Info
{
    public class AreaInfo : Shape
    {
        public readonly IList<Wall> Walls;
        public readonly IList<RoomType> RoomTypes;

        public AreaInfo(IList<Wall> walls, double margin, IList<RoomType> roomTypes) : base(walls)
        {
            Walls = walls;
            Margin = margin;
            RoomTypes = roomTypes;
        }
    }
}