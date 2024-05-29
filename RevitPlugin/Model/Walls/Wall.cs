using RevitPlugin.Geometry;

namespace RevitPlugin.Model
{
    public abstract class Wall
    {
        public readonly PointD startPoint;
        public readonly PointD endPoint;
        public readonly WallType WallType;

        public Wall(PointD startPoint, PointD endPoint, WallType wallType)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            WallType = wallType;
        }
    }
}