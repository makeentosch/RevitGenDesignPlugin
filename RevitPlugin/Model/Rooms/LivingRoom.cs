﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPlugin
{
    internal class LivingRoom : Room
    {
        public override string Name => "Living Room";

        protected override double MinWidthMeter => 3;

        protected override double MinHeightMeter => 3;

        protected override double MinSquareMeter => MinWidthMeter * MinHeightMeter;

        public override bool CanNearWindow => true;

        public LivingRoom() : base() { }

        public LivingRoom(XYZ minPoint) : base(minPoint) { }

        public LivingRoom(XYZ minPoint, double widthMeter = 0, double heightMeter = 0, double squareMeter = 0) :
            base(minPoint, widthMeter, heightMeter, squareMeter)
        { }

        public LivingRoom(Rectangle2D rectangle) : base(rectangle) { }

        public override Room CreateNew(Rectangle2D newRectangle)
        {
            return new LivingRoom(newRectangle);
        }

        public override Room CreateNew(XYZ pointMin, XYZ pointMax)
        {
            return new LivingRoom(new Rectangle2D(pointMin, pointMax));
        }

        public override bool IsCorrectPositionRelativeWalls(Side2D wallWithDoor, Side2D wallWithWindow)
        {
            return true;
        }
    }
}
