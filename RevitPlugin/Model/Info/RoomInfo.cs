using System.Collections.Generic;

namespace AreaRoomsAPI.Info
{
    internal class RoomInfo : Shape
    {
        public readonly RoomType roomType;

        public RoomInfo(IList<PointD> points, RoomType roomType) : base(points)
        {
            this.roomType = roomType;
        }
    }
}