using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RevitPlugin.Model
{
    public class AreaRoomsFormatsInfo
    {
        public readonly double Ratio = 0.5;

        public const double footsInMeter = 3.28084;

        private readonly ReadOnlyDictionary<RoomType, RoomFormat> formats;

        private static AreaRoomsFormatsInfo economyAreaRoomsFormatsInfo => new AreaRoomsFormatsInfo(
            new Dictionary<RoomType, RoomFormat>
            {
                { RoomType.Default, new RoomFormat(minWidth: 3, minSquare: 12) * footsInMeter },
                { RoomType.Bathroom, new RoomFormat(minWidth: 1.65, maxWidth: 1.85, minSquare: 4) * footsInMeter },
                { RoomType.Toilet, new RoomFormat(minWidth: 0.8, minSquare: 1) * footsInMeter },
                { RoomType.Kitchen, new RoomFormat(minWidth: 2.8, minSquare: 10) * footsInMeter },
                { RoomType.Loggia, new RoomFormat(minWidth: 0.8, maxWidth: 1.5, maxSquare: 3) * footsInMeter },
                { RoomType.Corridor, new RoomFormat(recWidth: 1.1, maxSquare: 5) * footsInMeter },
                { RoomType.Wardrobe, new RoomFormat(minWidth: 0.5, maxWidth: 1, minSquare: 0.25) * footsInMeter }
            });

        public RoomFormat this[RoomType roomType]
        {
            get { return formats[roomType]; }
        }

        public AreaRoomsFormatsInfo(IDictionary<RoomType, RoomFormat> formats)
        {
            this.formats = new ReadOnlyDictionary<RoomType, RoomFormat>(formats);
        }

        public static AreaRoomsFormatsInfo GetAreaFormatsInfo(AreaType areaType)
        {
            switch (areaType)
            {
                case AreaType.Economy:
                    return economyAreaRoomsFormatsInfo;

                default:
                    return economyAreaRoomsFormatsInfo;
            }
        }
    }
}