using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPlugin
{
    internal class Generate
    {
        private const double WidthOuterWall = 0.82; // в футах (250мм) 
        private const double WidthInnerWall = 0.49; // в футах (150мм)

        public static void GetShapes(FlatContour2D contourFlat, List<Room> rooms)
        {
            var contourWithoutWalls = GetContourWithoutWalls(contourFlat);
        }

        private static FlatContour2D GetContourWithoutWalls(FlatContour2D flatContour)
        {
            var minX = flatContour.GeometricShape.ExtremePoints.Min(p => p.X);
            var minY = flatContour.GeometricShape.ExtremePoints.Min(p => p.Y);
            var maxX = flatContour.GeometricShape.ExtremePoints.Max(p => p.X);
            var maxY = flatContour.GeometricShape.ExtremePoints.Max(p => p.Y);
            var points = flatContour.GeometricShape.ExtremePoints;
            var newExtremePoints = new XYZ[points.Length];

            for (var i = 0; i < points.Length; i++)
            {
                for (var x = -WidthOuterWall; x <= WidthOuterWall; x += WidthOuterWall)
                {
                    for (var y = -WidthOuterWall; y <= WidthOuterWall; y += WidthOuterWall)
                    {
                        if (x == 0 || y == 0) continue;

                        if (flatContour.GeometricShape.Contains(points[i] + new XYZ(x, y, 0)) &&
                            (points[i].X == minX || points[i].X == maxX || points[i].Y == minY || points[i].Y == maxY))
                            newExtremePoints[i] = points[i] + new XYZ(x, y, 0);
                        if (!flatContour.GeometricShape.Contains(points[i] + new XYZ(x, y, 0)) && points[i].X != minX && points[i].X != maxX
                            && points[i].Y != minY && points[i].Y != maxY)
                            newExtremePoints[i] = points[i] - new XYZ(x, y, 0);
                    }
                }
            }

            var newSideWithDoor = GetSideWithoutWalls(flatContour.SideWithDoor, points, newExtremePoints);
            var newSideWithWindow = GetSideWithoutWalls(flatContour.SideWithWindow, points, newExtremePoints);

            return new FlatCountour2D(new Rectangle2D(newExtremePoints), newSideWithDoor, newSideWithWindow);
        }

    }
}
