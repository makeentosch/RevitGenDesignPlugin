using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPlugin
{
    internal class Corridor : Room
    {
        public override string Name => "Corridor";

        protected override double MinWidthMeter => 1.1;

        protected override double MinHeightMeter => 1.1;

        protected override double MinSquareMeter => MinWidthMeter * MinHeightMeter;

        public override bool CanNearWindow => false;

        public Corridor() : base() { }

        public Corridor(XYZ minPoint) : base(minPoint) { }

        public Corridor(XYZ minPoint, double widthMeter = 0, double heightMeter = 0, double squareMeter = 0) :
            base(minPoint, widthMeter, heightMeter, squareMeter)
        { }

        public Corridor(Rectangle2D rectangle) : base(rectangle) { }

        public override Room CreateNew(Rectangle2D newRectangle)
        {
            return new Corridor(newRectangle);
        }

        public override Room CreateNew(XYZ pointMin, XYZ pointMax)
        {
            return new Corridor(new Rectangle2D(pointMin, pointMax));
        }

        public override bool IsCorrectPositionRelativeWalls(Side2D wallWithDoor, Side2D wallWithWindow)
        {
            return rectangle.ExtremePoints.Any(wallWithDoor.Contains);
        }
    }
}
