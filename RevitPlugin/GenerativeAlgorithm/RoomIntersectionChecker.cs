using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPlugin
{
    internal class RoomIntersectionChecker
    {
        internal static bool CheckIntersections(Room room, List<Room> spacedRooms)
        {
            var intersectionRooms = spacedRooms.Where(m => m.Rectangle.IntersectsWith(room.Rectangle)).ToList();

            switch (intersectionRooms.Count)
            {
                case 0:
                case 1 when intersectionRooms[0].Name == room.Name && (room.Rectangle.Contains(intersectionRooms[0].Rectangle) ||
                                                                       intersectionRooms[0].Rectangle.Contains(room.Rectangle)):
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Обрабатывает пересечения между комнатами
        /// </summary>
        /// <returns>Если данная комната не пересекается ни с одной другой
        /// комнатой в списке, то метод возвращает эту комнату. В противном случае он возвращает null</returns>>
        internal static Room ProcessingIntersections(Room room, List<Room> spacedRooms)
        {
            return !CheckIntersections(room, spacedRooms) ? room : null;
        }

        /// <summary>
        /// Проверяет, пересекается ли данный прямоугольник с любой из комнат в списке
        /// </summary>
        /// <returns>Если прямоугольник пересекается хотя бы с одной комнатой, то метод возвращает true. В противном случае он возвращает false</returns>
        internal static bool CheckRoomsOnRectangle(List<Room> rooms, Rectangle2D rectangle)
        {
            return rooms.Any(v => v.Rectangle.IntersectsWith(rectangle));
        }
    }
}
