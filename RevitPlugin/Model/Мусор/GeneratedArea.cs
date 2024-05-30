using AreaRoomsAPI.Info;
using System.Collections.Generic;

namespace AreaRoomsAPI
{
    public class GeneratedArea
    {
        public IList<(RoomType, IList<PointD>)> Rooms { get; set; }

        public GeneratedArea(IList<(RoomType, IList<PointD>)> rooms)
        {
            Rooms = rooms;
        }
    }
}