using AreaRoomsAPI.Info;
using System.Collections.Generic;

namespace AreaRoomsAPI
{
	public class GeneratedArea
	{
		public string Name { get; set; }

		public IList<(RoomType, IList<PointD>)> Rooms { get; set; }

		public GeneratedArea(IList<(RoomType, IList<PointD>)> rooms)
		{
			Rooms = rooms;
			if (rooms.Count > 0)
			{
				Name = rooms[0].Item1.ToString();
			}
		}
	}
}