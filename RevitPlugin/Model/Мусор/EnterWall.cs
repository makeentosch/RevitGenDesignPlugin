namespace AreaRoomsAPI.Info
{
    public class EnterWall : Wall
    {
        public readonly (double pos, double length) Enter;

        public EnterWall(PointD startPoint, PointD endPoint, (double start, double end) enter) : base(startPoint,
            endPoint, WallType.Enter)
        {
            Enter = enter;
        }
    }
}