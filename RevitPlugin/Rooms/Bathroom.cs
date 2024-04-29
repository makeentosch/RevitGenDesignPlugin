using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPlugin
{
    internal class Bathroom : Room
    {
        public override string Name => "Bathroom";

        protected override double MinWidthMeter => 1.65;

        protected override double MinHeightMeter => 1.65;

        protected override double MinSquareMeter => MinWidthMeter * MinHeightMeter;

        public override bool CanNearWindow => false;

        public Bathroom() : base() { }

        public Bathroom(XYZ minPoint) : base(minPoint) { }

        public Bathroom(XYZ minPoint, double widthMeter = 0, double heightMeter = 0, double squareMeter = 0) :
            base(minPoint, widthMeter, heightMeter, squareMeter)
        { }

        public Bathroom(Rectangle2D rectangle) : base(rectangle) { }

        public override Room CreateNew(Rectangle2D newRectangle)
        {
            return new Bathroom(newRectangle);
        }

        public override Room CreateNew(XYZ pointMin, XYZ pointMax)
        {
            return new Bathroom(new Rectangle2D(pointMin, pointMax));
        }

        public override bool IsCorrectPositionRelativeWalls(Side2D wallWithDoor, Side2D wallWithWindow)
        {
            return rectangle.ExtremePoints.All(point => !wallWithWindow.Contains(point));
        }
    }
}
