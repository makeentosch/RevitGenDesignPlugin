using System.Collections.Generic;
using RevitPlugin.Geometry;

namespace RevitPlugin.Model
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