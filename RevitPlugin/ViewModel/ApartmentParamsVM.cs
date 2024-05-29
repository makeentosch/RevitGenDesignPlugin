using Autodesk.Revit.DB;
using RevitPlugin.Model;
using System;
using System.Collections.Generic;
using RevitPlugin.View;
using System.Windows;
using System.Windows.Controls;

namespace RevitPlugin.ViewModel
{
    public class ApartmentParamsVM
    {
        private Curve balconyWall;
        private Curve entranceWall;
        private CurveLoop walls;
        private readonly Document document;
        public string RoomWidth { get; set; }
        public string AreaRoom { get; set; }
        public RadioButton FirstWidthBath { get; set; }
        public RadioButton SecondWidthBath { get; set; }
        public RadioButton ThirdWidthBath { get; set; }
        public string KitchenWidth { get; set; }
        public string AreaBath { get; set; }
        public string AreaKitchen { get; set; }
        public string ToiletteWidth { get; set; }
        public string AreaToilet { get; set; }
        public string CorridorWidth { get; set; }
        public string AreaCorridor { get; set; }
        public string LoggiaWidth { get; set; }
        public string AreaLoggia { get; set; }
        public string RoomsCount { get; set; }

        public ApartmentParamsVM(GeometryObject balconyWall, GeometryObject entranceWall, CurveLoop walls,
            Document document)
        {
            this.document = document;
            this.balconyWall = balconyWall as Curve;
            this.entranceWall = entranceWall as Curve;
            this.walls = walls;
        }
        public List<RoomType> GetRoomTypesByRoomsCount(int roomCount, bool isSeparatedToilet, bool isHaveLoggia,
            bool isHaveWardrobe)
        {
            var result = new List<RoomType>
            {
                RoomType.Corridor,
                RoomType.Kitchen,
                RoomType.Bathroom
            };

            for (var i = 0; i < roomCount; i++)
            {
                result.Add(RoomType.Default);
            }

            if (isSeparatedToilet) result.Add(RoomType.Toilet);
            if (isHaveLoggia) result.Add(RoomType.Loggia);
            if (isHaveWardrobe) result.Add(RoomType.Wardrobe);

            return result;
        }

        public AreaRoomsFormatsInfo GetAreaRoomFormatsInfo()
        {
            var formatsInfo = new Dictionary<RoomType, RoomFormat>();

            var types = Enum.GetValues(typeof(RoomType));

            foreach (var type in types)
            {
                formatsInfo.Add((RoomType)type, GetRoomFormatByRoomType((RoomType)type));
            }

            return new AreaRoomsFormatsInfo(formatsInfo);
        }

        public RoomFormat GetRoomFormatByRoomType(RoomType roomType)
        {
            var parameters = GetParametersByRoomType(roomType);

            var format = new RoomFormat(minWidth: parameters["minWidth"], minSquare: parameters["minArea"]);

            return format;
        }

        public Dictionary<string, double> GetParametersByRoomType(RoomType roomType)
        {
            var parameters = new Dictionary<string, double>();
            (string, string) textBoxes;
            switch (roomType)
            {
                case RoomType.Default:
                    textBoxes.Item1 = RoomWidth;
                    textBoxes.Item2 = AreaRoom;
                    break;
                case RoomType.Bathroom:
                    textBoxes.Item1 = GetDataFromRadioButtons(new List<RadioButton>
                        { FirstWidthBath, SecondWidthBath, ThirdWidthBath });
                    textBoxes.Item2 = AreaBath;
                    break;
                case RoomType.Kitchen:
                    textBoxes.Item1 = KitchenWidth;
                    textBoxes.Item2 = AreaKitchen;
                    break;
                case RoomType.Toilet:
                    textBoxes.Item1 = ToiletteWidth;
                    textBoxes.Item2 = AreaToilet;
                    break;
                case RoomType.Corridor:
                    textBoxes.Item1 = CorridorWidth;
                    textBoxes.Item2 = AreaCorridor;
                    break;
                case RoomType.Wardrobe:
                    textBoxes.Item1 = "0";
                    textBoxes.Item2 = "0";
                    break;
                case RoomType.Loggia:
                    textBoxes.Item1 = LoggiaWidth;
                    textBoxes.Item2 = AreaLoggia;
                    break;
                default:
                    textBoxes.Item1 = "0";
                    textBoxes.Item2 = "0";
                    break;
            }

            if (!double.TryParse(textBoxes.Item1, out _))
                textBoxes.Item1 = "0,0";
            parameters.Add("minWidth", double.Parse(textBoxes.Item1));
            if (!double.TryParse(textBoxes.Item2, out _))
                textBoxes.Item2 = "0,0";
            parameters.Add("minArea", double.Parse(textBoxes.Item2));

            return parameters;
        }

        public string GetDataFromRadioButtons(IList<RadioButton> buttons)
        {
            var result = "";
            foreach (RadioButton button in buttons)
            {
                if (button.IsChecked == true)
                {
                    result = (string)button.Content;
                }
            }

            return result;
        }

        public void GenerateRoom(object sender, RoutedEventArgs e)
        {
            var rooms = GetRoomTypesByRoomsCount(int.Parse(RoomsCount), false, false, false);
            var roomFormats = GetAreaRoomFormatsInfo();
            var roomsWindow = new Rooms(balconyWall, entranceWall, walls, document, rooms, roomFormats);
            roomsWindow.ShowDialog();
        }
    }
}
