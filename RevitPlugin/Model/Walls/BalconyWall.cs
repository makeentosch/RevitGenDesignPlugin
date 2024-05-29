using System.Collections.Generic;
using System.Linq;
using RevitPlugin.Geometry;

namespace RevitPlugin.Model
{
    public class BalconyWall : Wall
    {
        public readonly (double start, double end)[] Windows;

        public BalconyWall(PointD startPoint, PointD endPoint, IEnumerable<(double start, double end)> windows) : base(
            startPoint, endPoint, WallType.Balcony)
        {
            Windows = windows.ToArray();
        }
    }
}