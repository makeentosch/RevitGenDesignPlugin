using RevitPlugin.Geometry;

namespace RevitPlugin.Model
{
    public class StandartWall : Wall
    {
        public StandartWall(PointD startPoint, PointD endPoint) : base(startPoint, endPoint, WallType.Standart)
        {
        }
    }
}