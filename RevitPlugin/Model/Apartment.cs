using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RevitPlugin.Model
{
    public partial class Apartment
    {
        public Curve BalconyWall { get; set; }
        public Curve EntranceWall { get; set; }
        public CurveLoop Walls { get; set; }
        public Document Document { get; set; }
        public List<RoomType> RoomTypes { get; set; }
        public AreaRoomsFormatsInfo AreaRoomFormatsInfo { get; set; }

        public Apartment(Curve balconyWall, Curve entranceWall, CurveLoop walls, Document document)
        {
            // InitializeComponent(); ???
            BalconyWall = balconyWall;
            EntranceWall = entranceWall;
            Walls = walls;
            Document = document;
        }
    }
}
