//using AreaRoomsAPI.Geometric;
//using System;
//using System.Collections.Generic;

//namespace AreaRoomsAPI.Algorithm
//{
//    public struct RoomGene
//    {
//        public Point Point;
//        public int minXCell { get; private set; }
//        public int minYCell { get; private set; }
//        public int maxXCell { get; private set; }
//        public int maxYCell { get; private set; }
//        public int CellsWidth => maxXCell - minXCell + 1;
//        public int CellsHeight => maxYCell - minYCell + 1;

//        public double Area { get; private set; }

//        private double cellSize;

//        public RoomGene(Point point, double cellSize)
//        {
//            Point = point;
//            minXCell = point.X;
//            minYCell = point.Y;
//            maxXCell = point.X;
//            maxYCell = point.Y;
//            Area = cellSize * cellSize;
//            this.cellSize = cellSize;
//        }

//        public void AddCell(Point point)
//        {
//            minXCell = Math.Min(minXCell, point.X);
//            minYCell = Math.Min(minYCell, point.Y);
//            maxXCell = Math.Max(maxXCell, point.X);
//            maxYCell = Math.Max(maxYCell, point.Y);
//            Area += cellSize * cellSize;
//        }

//        public void AddCells(IEnumerable<Point> points)
//        {
//            foreach (var point in points)
//            {
//                AddCell(point);
//            }
//        }

//        public double GetWidth()
//        {
//            return (maxXCell - minXCell + 1) * cellSize;
//        }

//        public double GetHeight()
//        {
//            return (maxYCell - minYCell + 1) * cellSize;
//        }

//        public void ClearCells()
//        {
//            Area = cellSize * cellSize;
//            minXCell = Point.X;
//            minYCell = Point.Y;
//            maxXCell = Point.X;
//            maxYCell = Point.Y;
//        }

//        public static bool operator ==(RoomGene left, RoomGene right)
//        {
//            return left.Point == right.Point;
//        }

//        public static bool operator !=(RoomGene left, RoomGene right)
//        {
//            return left.Point != right.Point;
//        }

//        public override bool Equals(object obj)
//        {
//            if (!(obj is RoomGene roomGene))
//            {
//                return false;
//            }

//            return roomGene == this;
//        }

//        public override int GetHashCode()
//        {
//            return Point.GetHashCode();
//        }

//        public double GetMinWidth() => Math.Min(GetWidth(), GetHeight());

//        public double GetMaxWidth() => Math.Max(GetWidth(), GetHeight());
//    }
//}