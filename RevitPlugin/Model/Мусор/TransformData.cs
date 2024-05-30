using AreaRoomsAPI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace RevitPlugin
{
    public static class TransformData
    {
        public static IList<AreaRoomsAPI.Info.Wall> TransformAutodeskWallsToApi(Curve balconyWall,
            Curve entranceWall,
            List<Curve> walls,
            XYZ doorPosition,
            XYZ balconyPosition)
        {
            var result = new List<AreaRoomsAPI.Info.Wall>();
            var balconyStartPoint = new PointD(balconyWall.GetEndPoint(0).X, balconyWall.GetEndPoint(0).Y);
            var balconyEndPoint = new PointD(balconyWall.GetEndPoint(1).X, balconyWall.GetEndPoint(1).Y);

            var entranceStartPoint = new PointD(entranceWall.GetEndPoint(0).X, entranceWall.GetEndPoint(0).Y);
            var entranceEndPoint = new PointD(entranceWall.GetEndPoint(1).X, entranceWall.GetEndPoint(1).Y);
            var door = GetDoorPosition(doorPosition, entranceEndPoint, entranceStartPoint);

            foreach (var wall in walls)
            {
                var startPoint = new PointD(wall.GetEndPoint(0).X, wall.GetEndPoint(0).Y);
                var endPoint = new PointD(wall.GetEndPoint(1).X, wall.GetEndPoint(1).Y);

                if (startPoint == balconyStartPoint && endPoint == balconyEndPoint)
                    result.Add(new AreaRoomsAPI.Info.BalconyWall(startPoint, endPoint, new (double, double)[] { }));
                else if (startPoint == entranceStartPoint && endPoint == entranceEndPoint)
                    result.Add(new AreaRoomsAPI.Info.EnterWall(startPoint, endPoint, door));
                else
                    result.Add(new AreaRoomsAPI.Info.StandartWall(startPoint, endPoint));
            }

            return result;
        }

        private static (double, double) GetBalconyPosition(XYZ balconyPosition)
        {
            throw new NotImplementedException();
        }

        private static (double, double) GetDoorPosition(XYZ doorPosition, PointD entranceStart, PointD entanceEnd)
        {
            double middle;
            if (doorPosition.X == new XYZ().X && doorPosition.Y == new XYZ().Y && doorPosition.Z == new XYZ().Z)
                middle = GetMiddleCoord(entranceStart, entanceEnd);
            else
                middle = GetDoorMiddleCoord(doorPosition, entranceStart, entanceEnd);
            return (middle - 1.14829, middle + 1.14829);
        }

        private static double GetMiddleCoord(PointD start, PointD end)
        {
            if (start.X == end.X)
                return end.Y - (end.Y - start.Y) / 2;
            return end.X - (end.X - start.X) / 2;
        }

        private static double GetDoorMiddleCoord(XYZ doorPosition, PointD entranceStart, PointD entanceEnd)
        {
            if (entranceStart.X == entanceEnd.X)
                return doorPosition.Y;
            return doorPosition.X;
        }

        public static List<Curve> ParseCurveIterator(CurveLoopIterator curves)
        {
            var result = new List<Curve>();

            var isHaveNextElement = curves.MoveNext();
            while (isHaveNextElement)
            {
                result.Add(curves.Current);
                isHaveNextElement = curves.MoveNext();
            }

            return result;
        }
    }
}