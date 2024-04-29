//using System.Collections.Generic;
//using System.Linq;
//using AreaRoomsAPI.Geometric;
//using AreaRoomsAPI.Info;
//using Autodesk.Revit.DB;
//using GeneticSharp.Domain.Chromosomes;
//using GeneticSharp.Domain.Randomizations;

//namespace AreaRoomsAPI.Algorithm
//{
//    internal class AreaChromosome : ChromosomeBase
//    {
//        private readonly int cellsCount;
//        public readonly int cellsCountWidth;
//        public readonly int cellsCountHeight;
//        private readonly double cellSize;
//        public readonly double Width;
//        public readonly double Height;
//        private readonly double corridorSize;
//        private readonly AreaInfo areaInfo;
//        private readonly Point[] startPoints;
//        private List<Point>[] roomsPoints;

//        public AreaChromosome(AreaInfo areaInfo, double corridorSize, int roomsCount) : base(roomsCount)
//        {
//            this.areaInfo = areaInfo;
//            Width = areaInfo.Width;
//            Height = areaInfo.Height;
//            this.corridorSize = corridorSize;
//            startPoints = new Point[roomsCount];
//            roomsPoints = new List<Point>[roomsCount];
//            for (int i = 0; i < roomsCount; i++)
//            {
//                roomsPoints[i] = new List<Point>();
//            }

//            var nod = FindNOD(Width, Height);
//            cellSize = FindNOD(corridorSize, nod);
//            cellsCountWidth = (int)(Width / cellSize);
//            cellsCountHeight = (int)(Height / cellSize);

//            var coordinatesX = RandomizationProvider.Current.GetUniqueInts(roomsCount, 0, cellsCountWidth);
//            var coordinatesY = RandomizationProvider.Current.GetUniqueInts(roomsCount, 0, cellsCountHeight);

//            for (int i = 0; i < roomsCount; i++)
//            {
//                var point = new Point(coordinatesX[i], coordinatesY[i]);
//                startPoints[i] = point;
//                ReplaceGene(i, new Gene(new RoomGene(point, cellSize)));
//                AddCell(i, point);
//            }

//            cellsCount = cellsCountWidth * cellsCountHeight;
//        }

//        private double FindNOD(double a, double b)
//        {
//            if (b <= 0.05)
//            {
//                return a;
//            }

//            if (a < b)
//            {
//                return FindNOD(b, a);
//            }

//            return FindNOD(b, a % b);
//        }

//        public override IChromosome CreateNew()
//        {
//            return new AreaChromosome(areaInfo, corridorSize, Length);
//        }

//        public override Gene GenerateGene(int geneIndex)
//        {
//            return new Gene(new RoomGene(
//                new Point
//                (
//                    RandomizationProvider.Current.GetInt(0, cellsCountWidth),
//                    RandomizationProvider.Current.GetInt(0, cellsCountHeight)
//                ),
//                cellSize));
//        }

//        public override IChromosome Clone()
//        {
//            var clone = base.Clone() as AreaChromosome;

//            return clone;
//        }

//        public void AddCell(int geneIndex, Point point)
//        {
//            roomsPoints[geneIndex].Add(point);
//        }

//        public void ClearCells()
//        {
//            for (int i = 0; i < roomsPoints.Length; i++)
//            {
//                roomsPoints[i].Clear();
//            }
//        }

//        public double GetCellSize()
//        {
//            return cellSize;
//        }

//        public IList<PointD> ConvertPointListToPointDList(IList<PointD> points, PointD basePoint)
//        {
//            return points.Select(p => new PointD(basePoint.X + p.X, basePoint.Y + p.Y)).ToList();
//        }

//        public IList<IList<PointD>> GetRoomsBorders()
//        {
//            return roomsPoints.Select(x => GetRoomYLayers(x)).ToList();
//        }

//        private IList<PointD> GetRoomYLayers(IList<Point> points)
//        {
//            var ans = new SortedDictionary<int, IList<Point>>();
//            foreach (var point in points.OrderBy(p => p.Y))
//            {
//                if (!ans.ContainsKey(point.Y))
//                {
//                    ans.Add(point.Y, new List<Point>());
//                }

//                ans[point.Y].Add(point);
//            }

//            var fullAns = new SortedDictionary<int, IList<Point>>();
//            var prevPair = (new Point(0, 0), new Point(0, 0));
//            var flag = true;
//            var left = new List<PointD>();
//            var right = new List<PointD>();
//            var maxKey = ans.Keys.Max();
//            foreach (var key in ans.Keys)
//            {
//                if (flag)
//                {
//                    flag = false;
//                    prevPair = (new Point(ans[key].OrderBy(p => p.X).First()),
//                        new Point(ans[key].OrderBy(p => p.X).Last()));
//                    left.Add(new PointD(prevPair.Item1.X * cellSize, prevPair.Item1.Y * cellSize));
//                    right.Add(new PointD(prevPair.Item2.X * cellSize + cellSize, prevPair.Item2.Y * cellSize));
//                    continue;
//                }

//                var currentPair = (new Point(ans[key].OrderBy(p => p.X).First()),
//                    new Point(ans[key].OrderByDescending(p => p.X).First()));
//                if (currentPair.Item1.X != prevPair.Item1.X)
//                {
//                    left.Add(new PointD(prevPair.Item1.X * cellSize, currentPair.Item1.Y * cellSize));
//                    left.Add(new PointD(currentPair.Item1.X * cellSize, currentPair.Item1.Y * cellSize));
//                    prevPair.Item1 = new Point(currentPair.Item1);
//                }
//                else if (key == maxKey)
//                {
//                    left.Add(new PointD(currentPair.Item1.X * cellSize, currentPair.Item1.Y * cellSize + cellSize));
//                }

//                if (currentPair.Item2.X != prevPair.Item2.X)
//                {
//                    right.Add(new PointD(prevPair.Item2.X * cellSize + cellSize, currentPair.Item2.Y * cellSize));
//                    right.Add(new PointD(currentPair.Item2.X * cellSize + cellSize, currentPair.Item2.Y * cellSize));
//                    prevPair.Item2 = new Point(currentPair.Item2);
//                }
//                else if (key == maxKey)
//                {
//                    right.Add(new PointD(currentPair.Item2.X * cellSize + cellSize,
//                        currentPair.Item2.Y * cellSize + cellSize));
//                }
//            }

//            right.Reverse();
//            return left.Concat(right).ToList();
//        }

//        private IList<Point> GetSingleRoomBorder(SortedDictionary<int, IList<Point>> pointsLayers)
//        {
//            var left = new List<Point>();
//            var right = new List<Point>();
//            var flag = false;
//            var prevPair = (new Point(0, 0), new Point(0, 0));
//            var maxKey = pointsLayers.Keys.Max();
//            foreach (var key in pointsLayers.Keys)
//            {
//                if (!flag)
//                {
//                    flag = true;
//                    prevPair = (pointsLayers[key][0], pointsLayers[key][1]);
//                    left.Add(prevPair.Item1);
//                    right.Add(prevPair.Item2);
//                    continue;
//                }

//                var currentPair = (pointsLayers[key][0], pointsLayers[key][1]);

//                if (currentPair.Item1.X > prevPair.Item1.X)
//                {
//                    left.Add(new Point(currentPair.Item1.X, prevPair.Item1.Y));
//                    left.Add(currentPair.Item1);
//                    prevPair.Item1 = currentPair.Item1;
//                }
//                else if (currentPair.Item1.X < prevPair.Item1.X)
//                {
//                    left.Add(new Point(prevPair.Item1.X, currentPair.Item1.Y));
//                    left.Add(currentPair.Item1);
//                    prevPair.Item1 = currentPair.Item1;
//                }
//                else if (key == maxKey)
//                {
//                    left.Add(currentPair.Item1);
//                }

//                if (currentPair.Item2.X > prevPair.Item2.X)
//                {
//                    right.Add(new Point(prevPair.Item2.X, currentPair.Item2.Y));
//                    right.Add(currentPair.Item2);
//                    prevPair.Item2 = currentPair.Item2;
//                }
//                else if (currentPair.Item2.X < prevPair.Item2.X)
//                {
//                    right.Add(new Point(currentPair.Item2.X, prevPair.Item2.Y));
//                    right.Add(currentPair.Item2);
//                    prevPair.Item2 = currentPair.Item2;
//                }
//                else if (key == maxKey)
//                {
//                    right.Add(currentPair.Item2);
//                }
//            }

//            right.Reverse();
//            return left.Concat(right).ToList();
//        }

//        internal void AddCells(int geneIndex, IEnumerable<Point> points)
//        {
//            foreach (var point in points)
//            {
//                AddCell(geneIndex, point);
//            }
//        }
//    }
//}