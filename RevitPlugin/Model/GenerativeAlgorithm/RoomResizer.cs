using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPlugin
{
    internal class RoomResizer
    {
        /// <summary>
        /// Изменяет размеры и положение комнаты, создает новую комнату с заданными размерами и положением,
        /// проверяет, не пересекается ли она с другими комнатами, и если не пересекается, заменяет старую комнату новой
        /// </summary>
        internal static Room ResizeRoom(Room room, List<Room> rooms, Rectangle2D contourRectangle, XYZ pointMin, XYZ pointMax)
        {
            var newVisiting = room.CreateNew(new Rectangle2D(pointMin, pointMax));
            newVisiting = RoomIntersectionChecker.ProcessingIntersections(newVisiting, rooms);
            if (newVisiting != null)
                room = newVisiting;

            return room;
        }

        /// <summary>
        /// Уменьшает высоту комнаты, вычисляет, на сколько нужно уменьшить высоту, чтобы между комнатами было
        /// достаточно места для внутренней стены, затем проверяет, можно ли уменьшить высоту каждой из комнат на
        /// это значение, и если можно, изменяет размеры и положение комнаты, чтобы она соответствовала новому
        /// прямоугольнику. Если нельзя уменьшить высоту ни одной из комнат, метод удаляет обе комнаты из списка
        /// </summary>
        internal static void ReduceRoomHeight(List<Room> rooms, Room roomLeft, Room roomRight, Rectangle2D contourRectangle)
        {
            var sizeReduction = MainGenerativeAlgorithm.WidthInnerWall - (roomRight.Rectangle.MinXminY.X - roomLeft.Rectangle.MaxXmaxY.X);
            var indexLeft = rooms.IndexOf(roomLeft);
            var indexRight = rooms.IndexOf(roomRight);

            if (roomLeft.CanReduceHeightBy(sizeReduction))
                rooms[indexLeft] = ResizeRoom(roomLeft, rooms, contourRectangle, roomLeft.Rectangle.MinXminY,
                    roomLeft.Rectangle.MaxXmaxY - new XYZ(sizeReduction, 0, 0));
            else if (roomRight.CanReduceHeightBy(sizeReduction))
                rooms[indexRight] = ResizeRoom(roomRight, rooms, contourRectangle, roomRight.Rectangle.MinXminY + new XYZ(sizeReduction, 0, 0),
                    roomRight.Rectangle.MaxXmaxY);
            else
            {
                rooms[indexLeft] = null;
                rooms[indexRight] = null;
            }
        }

        /// <summary>
        /// Уменьшает ширину комнаты, вычисляет, на сколько нужно уменьшить ширину, чтобы между комнатами было
        /// достаточно места для внутренней стены, затем проверяет, можно ли уменьшить ширину каждой из комнат на
        /// это значение, и если можно, изменяет размеры и положение комнаты, чтобы она соответствовала новому
        /// прямоугольнику. Если нельзя уменьшить ширину ни одной из комнат, метод удаляет обе комнаты из списка
        /// </summary>
        internal static void ReduceRoomWidth(List<Room> rooms, Room roomBottom, Room roomTop, Rectangle2D contourRectangle)
        {
            var sizeReduction = MainGenerativeAlgorithm.WidthInnerWall - (roomTop.Rectangle.MinXminY.Y - roomBottom.Rectangle.MaxXmaxY.Y);
            var indexBottom = rooms.IndexOf(roomBottom);
            var indexTop = rooms.IndexOf(roomTop);

            if (roomBottom.CanReduceWidthBy(sizeReduction))
                rooms[indexBottom] = ResizeRoom(roomBottom, rooms, contourRectangle, roomBottom.Rectangle.MinXminY,
                    roomBottom.Rectangle.MaxXmaxY - new XYZ(0, sizeReduction, 0));
            else if (roomTop.CanReduceWidthBy(sizeReduction))
                rooms[indexTop] = ResizeRoom(roomTop, rooms, contourRectangle, roomTop.Rectangle.MinXminY + new XYZ(0, sizeReduction, 0),
                    roomTop.Rectangle.MaxXmaxY);
            else
            {
                rooms[indexBottom] = null;
                rooms[indexTop] = null;
            }
        }
    }
}
