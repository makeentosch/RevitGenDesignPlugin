using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPlugin
{
    internal class RoomPositionAdjuster
    {
        /// <summary>
        /// Вспомогательный метод для метода ProcessRoomsDistanceBorders. Для каждой стороны комнаты, если она не совпадает
        /// с соответствующей стороной контура, этот метод создает новый прямоугольник, сдвигает его к границе контура и проверяет,
        /// не пересекается ли он с другими комнатами. Если новый прямоугольник не пересекается с другими комнатами, то метод
        /// изменяет размеры и положение комнаты, чтобы она соответствовала новому прямоугольнику
        /// </summary>
        internal static void AdjustRoomPosition(List<Room> rooms, int index, Rectangle2D contourRectangle, double contourX, double contourY, double offsetX, double offsetY, string direction)
        {
            var room = rooms[index];
            var roomX = direction == "X" ? room.Rectangle.MinXminY.X : room.Rectangle.MaxXmaxY.X;
            var roomY = direction == "Y" ? room.Rectangle.MinXminY.Y : room.Rectangle.MaxXmaxY.Y;

            if (roomX == contourX && roomY == contourY) return;
            var rect = new Rectangle2D(new XYZ(room.Rectangle.MinXminY.X + offsetX, room.Rectangle.MinXminY.Y + offsetY, room.Rectangle.MinXminY.Z),
                room.Rectangle.MaxXmaxY - new XYZ(offsetX, offsetY, 0));

            if (!RoomIntersectionChecker.CheckRoomsOnRectangle(rooms, rect))
            {
                rooms[index] = room.CreateNew(new XYZ(room.Rectangle.MinXminY.X + offsetX, room.Rectangle.MinXminY.Y + offsetY, room.Rectangle.MinXminY.Z),
                    room.Rectangle.MaxXmaxY - new XYZ(offsetX, offsetY, 0));
            }
        }

        /// <summary>
        /// Фильтрует комнаты, которые находятся между minX и maxX,
        /// исключает комнату, которую мы сравниваем, и комнаты, которые не пересекаются с данной комнатой по оси Y,
        /// затем сортирует оставшиеся комнаты по расстоянию до minX или maxX и возвращает ближайшую комнату
        /// </summary>
        /// <returns>Ближайшая комната по оси X</returns>
        internal static Room GetNearestRoomAxisX(Room room, List<Room> rooms, double minX, double maxX)
        {
            return rooms.Where(v => minX <= v.Rectangle.MinXminY.X && v.Rectangle.MinXminY.X <= maxX)
                .Where(v => v.Name != room.Name && v.Rectangle.MinXminY != room.Rectangle.MinXminY)
                .Where(v => MainGenerativeAlgorithm.CheckBoundsOnY(room.Rectangle, v.Rectangle))
                .OrderBy(v =>
                {
                    if (room.Rectangle.MinXminY.X == maxX)
                        return maxX - v.Rectangle.MaxXmaxY.X;
                    return v.Rectangle.MinXminY.X - minX;
                })
                .FirstOrDefault();
        }

        /// <summary>
        /// Фильтрует комнаты, которые находятся между minY и maxY,
        /// исключает комнату, которую мы сравниваем, и комнаты, которые не пересекаются с данной комнатой по оси Y,
        /// затем сортирует оставшиеся комнаты по расстоянию до minX или maxX и возвращает ближайшую комнату
        /// </summary>
        /// <returns>Ближайшая комната по оси Y</returns>
        internal static Room GetNearestRoomAxisY(Room room, List<Room> rooms, double minY, double maxY)
        {
            return rooms.Where(v => minY <= v.Rectangle.MinXminY.Y && v.Rectangle.MinXminY.Y <= maxY)
                .Where(v => v.Name != room.Name && v.Rectangle.MinXminY != room.Rectangle.MinXminY)
                .Where(v => MainGenerativeAlgorithm.CheckBoundsOnX(room.Rectangle, v.Rectangle))
                .OrderBy(v =>
                {
                    if (room.Rectangle.MinXminY.Y == maxY)
                        return maxY - v.Rectangle.MaxXmaxY.Y;
                    return v.Rectangle.MinXminY.Y - minY;
                })
                .FirstOrDefault();
        }
    }
}
