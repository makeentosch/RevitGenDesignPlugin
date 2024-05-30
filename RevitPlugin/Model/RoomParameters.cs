using AreaRoomsAPI.Info;

namespace RevitPlugin.Model
{
	public class RoomParameters
	{
		public RoomType RoomType { get; set; }
		public string Name { get; set; } = "Комната";
		public double Width { get; set; } = 3;
		public double Area { get; set; } = 12;
		public double Ratio { get; set; } = 0.5;

		public RoomParameters() { }

		public RoomParameters(string name, double width, double area, double ratio, RoomType roomType)
		{
			Name = name;
			Width = width;
			Area = area;
			Ratio = ratio;
			RoomType = roomType;
		}
	}
}
