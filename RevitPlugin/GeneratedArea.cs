using RevitPlugin.Geometry;
using RevitPlugin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPlugin
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
