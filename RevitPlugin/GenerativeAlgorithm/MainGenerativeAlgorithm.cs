using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;

namespace RevitPlugin
{
    internal class MainGenerativeAlgorithm
    {
        internal const double WidthOuterWall = 0.82; // в футах (250мм) 
        internal const double WidthInnerWall = 0.49; // в футах (150мм)
        
        /// <summary>
        /// Генерирует комнаты внутри квартиры
        /// </summary>
        public static List<List<Room>> GetShapes(ContourFlat2D contourFlat, List<Room> rooms)
        {
            var contourWithoutWalls = GetContourWithoutWalls(contourFlat);
            var contourRectangle = (Rectangle2D)contourWithoutWalls.GeometricShape;
            var height = contourRectangle.Width.LengthOnMeter;
            var width = contourRectangle.Height.LengthOnMeter;
            var squareSpace = height * width;
            var sortRooms = rooms.OrderByDescending(x => x.SquareMeter).ToList();

            return MoveRooms(sortRooms, contourWithoutWalls);
        }

        /// <summary>
        /// Получает контур квартиры без учета стен - сначала получает крайние точки контура,
        /// затем вычисляет новые крайние точки, учитывая ширину стен, и создает новый контур с этими точками
        /// </summary>
        private static ContourFlat2D GetContourWithoutWalls(ContourFlat2D contourFlat)
        {
            var minX = contourFlat.GeometricShape.ExtremePoints.Min(p => p.X);
            var minY = contourFlat.GeometricShape.ExtremePoints.Min(p => p.Y);
            var maxX = contourFlat.GeometricShape.ExtremePoints.Max(p => p.X);
            var maxY = contourFlat.GeometricShape.ExtremePoints.Max(p => p.Y);
            var points = contourFlat.GeometricShape.ExtremePoints;
            var newExtremePoints = new XYZ[points.Length];

            for (var i = 0; i < points.Length; i++)
            {
                for (var x = -WidthOuterWall; x <= WidthOuterWall; x += WidthOuterWall)
                {
                    for (var y = -WidthOuterWall; y <= WidthOuterWall; y += WidthOuterWall)
                    {
                        if (x == 0 || y == 0) continue;

                        if (contourFlat.GeometricShape.Contains(points[i] + new XYZ(x, y, 0)) &&
                            (points[i].X == minX || points[i].X == maxX || points[i].Y == minY || points[i].Y == maxY))
                            newExtremePoints[i] = points[i] + new XYZ(x, y, 0);
                        if (!contourFlat.GeometricShape.Contains(points[i] + new XYZ(x, y, 0)) && points[i].X != minX && points[i].X != maxX
                            && points[i].Y != minY && points[i].Y != maxY)
                            newExtremePoints[i] = points[i] - new XYZ(x, y, 0);
                    }
                }
            }

            var newSideWithDoor = GetSideWithoutWalls(contourFlat.SideWithDoor, points, newExtremePoints);
            var newSideWithWindow = GetSideWithoutWalls(contourFlat.SideWithWindow, points, newExtremePoints);

            return new ContourFlat2D(new Rectangle2D(newExtremePoints), newSideWithDoor, newSideWithWindow);
        }

        /// <summary>
        /// Получает стороны контура без учета стен - находит новые точки для начала и конца стороны,
        /// учитывая ширину стен, и создает новую сторону с этими точками
        /// </summary>
        private static Side2D GetSideWithoutWalls(Side2D side, XYZ[] points, XYZ[] pointsWithoutWalls)
        {
            var newPointMin = new XYZ();
            var newPointMax = new XYZ();

            for (var i = 0; i < points.Length; i++)
            {
                if (side.pointMin == points[i])
                    newPointMin = pointsWithoutWalls[i];
                else if (side.pointMax == points[i])
                    newPointMax = pointsWithoutWalls[i];
            }

            return new Side2D(newPointMin, newPointMax);
        }

        /// <summary>
        /// Перемещает комнаты внутри контура - получает все возможные варианты
        /// расположения комнат, затем для каждого варианта обрабатывает расстояния до границ и размеры помещений
        /// и, если вариант соответствует всем условиям, добавляет его в результат
        /// </summary>
        private static List<List<Room>> MoveRooms(List<Room> rooms, ContourFlat2D contourFlat)
        {
            var contourRectangle = (Rectangle2D)contourFlat.GeometricShape;
            var variants = GetVariantsFlats(rooms, contourFlat);
            var result = new List<List<Room>>();

            foreach (var t in variants)
            {
                for (var j = 0; j < t.Count; j++)
                {
                    ProcessRoomsDistanceBorders(t, j, contourRectangle);
                    ProcessSizesPlacedRooms(t, j, contourRectangle);
                }

                if (CheckCompliance(result, t, contourFlat))
                    result.Add(t);
            }

            return result;
        }

        /// <summary>
        /// Проверяет, соответствует ли вариант расположения комнат всем условиям,
        /// что все комнаты были размещены, и что все комнаты корректно расположены относительно стен с дверью и окном
        /// </summary>
        private static bool CheckCompliance(List<List<Room>> flatVariants, List<Room> rooms, ContourFlat2D contourFlat)
        {
            if (rooms.Any(r => r == null) ||
                !rooms.All(r => r.IsCorrectPositionRelativeWalls(contourFlat.SideWithDoor, contourFlat.SideWithWindow)))
                return false;


            var previousSimilar = true;

            for (var i = flatVariants.Count - 1; i >= 0; i--)
            {
                for (var j = 0; j < flatVariants[i].Count; j++)
                {
                    if (rooms[j] == null || flatVariants[i][j].Name != rooms[j].Name) continue;
                    previousSimilar = flatVariants[i][j].Rectangle.Contains(rooms[j].Rectangle);
                }

                if (previousSimilar) return false;
            }

            return true;
        }

        /// <summary>
        /// Получает все возможные варианты расположения комнат. Сначала заполняет первую комнату,
        /// затем для каждого варианта добавляет следующую комнату и так далее, пока не будут добавлены все комнаты
        /// </summary>
        private static List<List<Room>> GetVariantsFlats(List<Room> rooms, ContourFlat2D contourFlat)
        {
            var result = new List<List<Room>>();

            for (var i = 0; i < rooms.Count; i++)
            {
                if (result.Count == 0)
                {
                    FillRoom(result, new List<Room>(), rooms[i], contourFlat, rooms.Count(r => r.Name == rooms[i].Name));
                    continue;
                }
                foreach (var t in result)
                    FillRoom(result, t, rooms[i], contourFlat, rooms.Count(r => r.Name == rooms[i].Name));

                result = result.Where(r => r.Count == i + 1).ToList();
            }

            return result.Where(v => v.Count >= rooms.Count - 1).ToList();
        }

        /// <summary>
        /// Добавляет комнату в вариант расположения. Сначала получает все возможные позиции для комнаты, затем для каждой позиции
        /// создает новую комнату, проверяет, что она помещается в контур, обрабатывает пересечения с другими комнатами
        /// и, если комната корректна, добавляет ее в вариант
        /// </summary>
        private static void FillRoom(List<List<Room>> roomVariants, List<Room> workVariant, Room room, ContourFlat2D contourFlat,
            int countRoomInVariant)
        {
            var contourRectangle = (Rectangle2D)contourFlat.GeometricShape;
            var possiblePosition = GetPossiblePosition(workVariant, contourRectangle, room);

            foreach (var position in possiblePosition)
            {
                var newRoom = room.CreateNew(new Rectangle2D(position, room.WidthFeet, room.HeightFeet));

                if (!contourRectangle.Contains(newRoom.Rectangle)) 
                    continue;
                newRoom = RoomIntersectionChecker.ProcessingIntersections(newRoom, workVariant);
                if (newRoom == null || workVariant.Count(r => r.Name == newRoom.Name) >= countRoomInVariant) 
                    continue;
                var newVariant = new List<Room>();
                newVariant.AddRange(workVariant.ToArray());
                newVariant.Add(newRoom);
                roomVariants.Add(newVariant);
            }
        }

        /// <summary>
        /// Получает все возможные позиции для комнаты. Сначала получает позиции относительно контура,
        /// затем добавляет позиции относительно каждой уже размещенной комнаты
        /// </summary>
        private static List<XYZ> GetPossiblePosition(List<Room> rooms, Rectangle2D contourRectangle, Room room)
        {
            var result = new List<XYZ>();
            result.AddRange(GetPossiblePositionsContour(room, contourRectangle));

            foreach (var t in rooms)
            {
                result.AddRange(GetPossiblePositionsRoom(t.Rectangle, room));
            }

            return result;
        }

        /// <summary>
        /// Получает все возможные позиции для комнаты относительно контура
        /// </summary>
        /// <returns>Возвращает список из четырех позиций: в каждом углу контура</returns>
        private static IEnumerable<XYZ> GetPossiblePositionsContour(Room room, Rectangle2D contourRectangle)
        {
            return new List<XYZ>
            {
                contourRectangle.MinXminY,
                contourRectangle.MaxXminY - new XYZ(room.WidthFeet, 0, 0),
                new XYZ(contourRectangle.MaxXmaxY.X - room.WidthFeet, contourRectangle.MaxXmaxY.Y - room.HeightFeet, contourRectangle.MinXminY.Z),
                contourRectangle.MinXmaxY - new XYZ(0, room.WidthFeet, 0),
            };
        }

        /// <summary>
        /// Получает все возможные позиции для комнаты относительно заданного прямоугольника в контуре. Создает
        /// список новых точек вокруг прямоугольника, учитывая ширину внутренней стены, затем для каждой точки создает
        /// дополнительные точки вокруг нее с шагом, равным ширине и высоте комнаты, и добавляет их в результат,
        /// если они еще не были добавлены
        /// </summary>
        private static List<XYZ> GetPossiblePositionsRoom(Rectangle2D rectangleInContour, Room room)
        {
            var result = new HashSet<XYZ>();

            var newPoints = new List<XYZ>
            {
                rectangleInContour.MinXminY + new XYZ(-WidthInnerWall, -WidthInnerWall, 0),
                rectangleInContour.MinXminY + new XYZ(-WidthInnerWall, 0, 0),
                rectangleInContour.MinXminY + new XYZ(0, -WidthInnerWall, 0),
                rectangleInContour.MaxXminY + new XYZ(WidthInnerWall, -WidthInnerWall, 0),
                rectangleInContour.MaxXminY + new XYZ(WidthInnerWall, 0, 0),
                rectangleInContour.MaxXminY + new XYZ(0, -WidthInnerWall, 0),
                rectangleInContour.MaxXmaxY + new XYZ(WidthInnerWall, WidthInnerWall, 0),
                rectangleInContour.MaxXmaxY + new XYZ(WidthInnerWall, 0, 0),
                rectangleInContour.MaxXmaxY + new XYZ(0, WidthInnerWall, 0),
                rectangleInContour.MinXmaxY + new XYZ(-WidthInnerWall, WidthInnerWall, 0),
                rectangleInContour.MinXmaxY + new XYZ(0, WidthInnerWall, 0),
                rectangleInContour.MinXmaxY + new XYZ(-WidthInnerWall, 0, 0)
            };

            foreach (var point in newPoints)
            {
                for (var i = -room.WidthFeet; i <= room.WidthFeet; i += room.WidthFeet)
                {
                    for (var j = -room.HeightFeet; j <= room.HeightFeet; j += room.HeightFeet)
                    {
                        var newPosition1 = point + new XYZ(i, j, 0);

                        if (!result.Contains(newPosition1))
                            result.Add(newPosition1);
                    }
                }
            }

            return result.ToList();
        }

        /// <summary>
        /// Проверяет, пересекается ли данная комната с любой из других комнат в списке
        /// </summary>
        /// <returns>Если комната не
        /// пересекается ни с одной другой комнатой, или если она пересекается только с одной другой комнатой
        /// того же типа и одна комната полностью содержится в другой, то метод возвращает false. В противном
        /// случае он возвращает true</returns>
        

        /// <summary>
        /// Обрабатывает расстояния между комнатами и границами контура
        /// </summary>
        private static void ProcessRoomsDistanceBorders(List<Room> rooms, int index, Rectangle2D contourRectangle)
        {
            if (rooms.Any(r => r == null)) return;
            RoomPositionAdjuster.AdjustRoomPosition(rooms, index, contourRectangle, contourRectangle.MinXminY.X, contourRectangle.MinXminY.Y, 0.1, 0, "X");
            RoomPositionAdjuster.AdjustRoomPosition(rooms, index, contourRectangle, contourRectangle.MaxXmaxY.X, contourRectangle.MaxXmaxY.Y, -0.1, 0, "X");
            RoomPositionAdjuster.AdjustRoomPosition(rooms, index, contourRectangle, contourRectangle.MinXminY.X, contourRectangle.MinXminY.Y, 0, 0.1, "Y");
            RoomPositionAdjuster.AdjustRoomPosition(rooms, index, contourRectangle, contourRectangle.MaxXmaxY.X, contourRectangle.MaxXmaxY.Y, 0, -0.1, "Y");
        }

        /// <summary>
        /// Обрабатывает размеры размещенных комнат. Для каждой стороны комнаты, если она не совпадает с соответствующей
        /// стороной контура, этот метод находит ближайшую комнату по оси X или Y, проверяет, достаточно ли места между комнатами
        /// для внутренней стены, и если достаточно, изменяет размеры и положение комнаты, чтобы она соответствовала новому
        /// прямоугольнику. Если места недостаточно, метод уменьшает высоту или ширину комнаты
        /// </summary>
        private static void ProcessSizesPlacedRooms(List<Room> rooms, int index, Rectangle2D contourRectangle)
        {
            if (rooms.All(r => r != null) && rooms[index].Rectangle.MinXminY.X != contourRectangle.MinXminY.X)
            {
                var nearestRoom = RoomPositionAdjuster.GetNearestRoomAxisX(rooms[index], rooms, contourRectangle.MinXminY.X, rooms[index].Rectangle.MinXminY.X);
                if (nearestRoom != null)
                    if (rooms[index].Rectangle.MinXminY.X - nearestRoom.Rectangle.MaxXmaxY.X >= WidthInnerWall)
                        rooms[index] = RoomResizer.ResizeRoom(rooms[index], rooms, contourRectangle,
                            new XYZ(nearestRoom.Rectangle.MaxXmaxY.X + WidthInnerWall, rooms[index].Rectangle.MinXminY.Y, rooms[index].Rectangle.MinXminY.Z),
                            rooms[index].Rectangle.MaxXmaxY);
                    else
                        RoomResizer.ReduceRoomHeight(rooms, nearestRoom, rooms[index], contourRectangle);
            }
            if (rooms.All(r => r != null) && rooms[index].Rectangle.MaxXmaxY.X != contourRectangle.MaxXmaxY.X)
            {
                var nearestRoom = RoomPositionAdjuster.GetNearestRoomAxisX(rooms[index], rooms, rooms[index].Rectangle.MaxXmaxY.X, contourRectangle.MaxXmaxY.X);
                if (nearestRoom != null)
                    if (nearestRoom.Rectangle.MinXminY.X - rooms[index].Rectangle.MaxXmaxY.X >= WidthInnerWall)
                        rooms[index] = RoomResizer.ResizeRoom(rooms[index], rooms, contourRectangle, rooms[index].Rectangle.MinXminY,
                            new XYZ(nearestRoom.Rectangle.MinXminY.X - WidthInnerWall, rooms[index].Rectangle.MaxXmaxY.Y, rooms[index].Rectangle.MaxXmaxY.Z));
                    else
                        RoomResizer.ReduceRoomHeight(rooms, rooms[index], nearestRoom, contourRectangle);
            }
            if (rooms.All(r => r != null) && rooms[index].Rectangle.MinXminY.Y != contourRectangle.MinXminY.Y)
            {
                var nearestRoom = RoomPositionAdjuster.GetNearestRoomAxisY(rooms[index], rooms, contourRectangle.MinXminY.Y, rooms[index].Rectangle.MinXminY.Y);
                if (nearestRoom != null)
                    if (rooms[index].Rectangle.MinXminY.Y - nearestRoom.Rectangle.MaxXmaxY.Y >= WidthInnerWall)
                        rooms[index] = RoomResizer.ResizeRoom(rooms[index], rooms, contourRectangle,
                            new XYZ(rooms[index].Rectangle.MinXminY.X, nearestRoom.Rectangle.MaxXmaxY.Y + WidthInnerWall, rooms[index].Rectangle.MinXminY.Z),
                            rooms[index].Rectangle.MaxXmaxY);
                    else
                        RoomResizer.ReduceRoomWidth(rooms, nearestRoom, rooms[index], contourRectangle);
            }

            if (rooms.Any(r => r == null) || rooms[index].Rectangle.MaxXmaxY.Y == contourRectangle.MaxXmaxY.Y) return;
            {
                var nearestRoom = RoomPositionAdjuster.GetNearestRoomAxisY(rooms[index], rooms, rooms[index].Rectangle.MaxXmaxY.Y, contourRectangle.MaxXmaxY.Y);
                if (nearestRoom == null) return;
                if (nearestRoom.Rectangle.MinXminY.Y - rooms[index].Rectangle.MaxXmaxY.Y >= WidthInnerWall)
                    rooms[index] = RoomResizer.ResizeRoom(rooms[index], rooms, contourRectangle, rooms[index].Rectangle.MinXminY,
                        new XYZ(rooms[index].Rectangle.MaxXmaxY.X, nearestRoom.Rectangle.MinXminY.Y - WidthInnerWall, rooms[index].Rectangle.MaxXmaxY.Z));
                else
                    RoomResizer.ReduceRoomWidth(rooms, rooms[index], nearestRoom, contourRectangle);
            }
        }

        /// <summary>
        /// Проверяет, пересекаются ли два прямоугольника по оси Y
        /// </summary>
        /// <returns>Если одна из сторон первого прямоугольника совпадает с одной из сторон второго прямоугольника, или если первый
        /// прямоугольник полностью содержится во втором прямоугольнике по оси Y, то метод возвращает true. В противном случае
        /// он возвращает false</returns>
        internal static bool CheckBoundsOnY(Rectangle2D rectangle1, Rectangle2D rectangle2)
        {
            if (rectangle1.MinXminY.Y == rectangle2.MinXminY.Y || rectangle1.MaxXmaxY.Y == rectangle2.MaxXmaxY.Y)
                return true;
            if (rectangle1.MinXminY.Y > rectangle2.MinXminY.Y)
                return rectangle1.MinXminY.Y <= rectangle2.MaxXmaxY.Y;
            return rectangle1.MaxXmaxY.Y >= rectangle2.MinXminY.Y;
        }

        /// <summary>
        /// Проверяет, пересекаются ли два прямоугольника по оси X
        /// </summary>
        /// <returns>Если одна из сторон первого прямоугольника совпадает с одной из сторон второго прямоугольника, или если первый
        /// прямоугольник полностью содержится во втором прямоугольнике по оси X, то метод возвращает true. В противном случае
        /// он возвращает false</returns>
        internal static bool CheckBoundsOnX(Rectangle2D rectangle1, Rectangle2D rectangle2)
        {
            if (rectangle1.MinXminY.X == rectangle2.MinXminY.X || rectangle1.MaxXmaxY.X == rectangle2.MaxXmaxY.X)
                return true;
            if (rectangle1.MinXminY.X > rectangle2.MinXminY.X)
                return rectangle1.MinXminY.X <= rectangle2.MaxXmaxY.X;
            return rectangle1.MaxXmaxY.X >= rectangle2.MinXminY.X;
        }
    }
}