using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPlugin.Model
{
	public class RoomParameters
	{
		public string Name { get; set; }
		public double Width { get; set; } = 3;
		public double Area { get; set; } = 12;
		public double Ratio { get; set; } = 0.5;

		public RoomParameters() { }

		public RoomParameters(string name, double width, double area, double ratio)
		{
			Name = name;
			Width = width;
			Area = area;
			Ratio = ratio;
		}
	}
}
