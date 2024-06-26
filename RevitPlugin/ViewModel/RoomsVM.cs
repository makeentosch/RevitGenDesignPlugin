﻿using Autodesk.Revit.DB;
using RevitPlugin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using RevitPlugin.API;

namespace RevitPlugin.ViewModel
{
    public class RoomsVM
    {
        public readonly Curve balconyWall;
        public readonly Curve entranceWall;
        public readonly List<Curve> walls;
        public readonly List<RoomType> rooms;
        public readonly AreaRoomsFormatsInfo roomsFormats;
        public readonly XYZ leftTopPoint;
        public readonly IList<GeneratedArea> generatedRooms;
        public readonly Document document;
        public readonly List<Canvas> allCanvas;
        public readonly double appartmentWidth;
        public readonly double appartmentHeight;
        public int currentAppartment = 0;
        public Canvas FirstPreview { get; set; }
        public Canvas SecondPreview { get; set; }
        public Canvas ThirdPreview { get; set; }
        public Canvas FourthPreview { get; set; }
        public Canvas FifthPreview { get; set; }
        public Canvas SixthPreview { get; set; }
        public Canvas RoomCanvas { get; set; }

        public RoomsVM(GeometryObject balconyWall, GeometryObject entranceWall, CurveLoop walls,
            Document document, List<RoomType> rooms, AreaRoomsFormatsInfo roomsFormats)
        {
            this.balconyWall = balconyWall as Curve;
            this.entranceWall = entranceWall as Curve;
            this.walls = TransformData.ParseCurveIterator(walls.GetCurveLoopIterator());
            this.document = document;
            this.rooms = rooms;
            this.roomsFormats = roomsFormats;
            leftTopPoint = GetLeftAndRightPoints(this.walls).Item1;
            (appartmentWidth, appartmentHeight) = GetWidthAndHeight();
            allCanvas = GetAllCanvs();
            generatedRooms = GenerateRooms();
            DrawAllCanvas();
        }

        public IList<GeneratedArea> GenerateRooms()
        {
            var areaInfo = new AreaInfo(
                TransformData.TransformAutodeskWallsToApi(balconyWall, entranceWall, walls, new XYZ(), new XYZ()),
                0.0, rooms
            );

            var roomsGenerator = new RoomsGenerator(areaInfo, roomsFormats);

            return roomsGenerator.GenerateAreas(6);
        }

        public void DrawAllCanvas()
        {
            var mulitpier = GetMultipier(RoomCanvas, appartmentWidth, appartmentHeight);
            var withDelta = GetDeltaCoordinates(mulitpier, RoomCanvas.Width, appartmentWidth);
            var heightDelta = GetDeltaCoordinates(mulitpier, RoomCanvas.Height, appartmentHeight);
            DrawAppartment(RoomCanvas, mulitpier, withDelta, heightDelta, 0);
            for (var i = 0; i < allCanvas.Count; i++)
            {
                mulitpier = GetMultipier(allCanvas[i], appartmentWidth, appartmentHeight);
                withDelta = GetDeltaCoordinates(mulitpier, allCanvas[i].Width, appartmentWidth);
                heightDelta = GetDeltaCoordinates(mulitpier, allCanvas[i].Height, appartmentHeight);
                DrawAppartment(allCanvas[i], mulitpier, withDelta, heightDelta, i);
            }
        }

        public void DrawAppartment(Canvas canvas, double multipier, double widthDelta, double heightDelta,
            int appartmentIndex)
        {
            foreach (var pair in generatedRooms[appartmentIndex].Rooms)
            {
                for (var i = 0; i < pair.Item2.Count; i++)
                {
                    var startPoint = pair.Item2[i];
                    var endPoint = pair.Item2[(i + 1) % pair.Item2.Count];

                    var line = new System.Windows.Shapes.Line
                    {
                        X1 = (startPoint.X - leftTopPoint.X) * multipier + widthDelta,
                        Y1 = (startPoint.Y - leftTopPoint.Y) * multipier + heightDelta,
                        X2 = (endPoint.X - leftTopPoint.X) * multipier + widthDelta,
                        Y2 = (endPoint.Y - leftTopPoint.Y) * multipier + heightDelta,
                        Stroke = Brushes.Black,
                    };

                    canvas.Children.Add(line);
                }
            }
        }

        public void CreateAppartment(int appartmentIndex)
        {
            var curves = new List<List<Curve>>();
            foreach (var pair in generatedRooms[appartmentIndex].Rooms)
            {
                curves.Add(AutodeskAPICreator.GetCurvesByPoints(pair.Item2, document));
            }

            AutodeskAPICreator.CreateRooms(curves, document);
        }

        public (XYZ, XYZ) GetLeftAndRightPoints(List<Curve> curves)
        {
            var leftCurve = curves[0];
            var rightCurve = curves[0];

            for (var i = 0; i < curves.Count; i++)
            {
                var point = curves[i].GetEndPoint(0);
                var leftPoint = leftCurve.GetEndPoint(0);
                var rightPoint = rightCurve.GetEndPoint(0);

                if (point.X < leftPoint.X && point.Y < leftPoint.Y)
                {
                    leftCurve = curves[i];
                }

                if (point.X > rightPoint.X && point.Y > rightPoint.Y)
                {
                    rightCurve = curves[i];
                }
            }

            return (leftCurve.GetEndPoint(0), rightCurve.GetEndPoint(0));
        }

        public (double, double) GetWidthAndHeight()
        {
            var widthAndHeight = (0.0, 0.0);
            foreach (var curve in walls)
            {
                var startPoint = curve.GetEndPoint(0);
                var endPoint = curve.GetEndPoint(1);
                if (startPoint.X == endPoint.X &&
                    (IsXYZEquals(startPoint, leftTopPoint) || IsXYZEquals(endPoint, leftTopPoint)))
                {
                    widthAndHeight.Item2 = curve.ApproximateLength;
                }
                else if (startPoint.Y == endPoint.Y &&
                         (IsXYZEquals(startPoint, leftTopPoint) || IsXYZEquals(endPoint, leftTopPoint)))
                {
                    widthAndHeight.Item1 = curve.ApproximateLength;
                }
            }

            return widthAndHeight;
        }

        public bool IsXYZEquals(XYZ first, XYZ second)
        {
            return first.X == second.X && first.Y == second.Y && first.Z == second.Z;
        }

        public double GetMultipier(Canvas canvas, double width, double height)
        {
            var canvasWidth = canvas.Width;
            var canvasHeight = canvas.Height;
            var multipier = 0.0;
            do
            {
                multipier += 0.5;
            } while (width * multipier < canvasWidth && height * multipier < canvasHeight);

            return multipier - 0.5;
        }

        public double GetDeltaCoordinates(double multipier, double canvasLength, double length)
        {
            return (canvasLength - multipier * length) / 2;
        }

        public List<Canvas> GetAllCanvs()
        {
            return new List<Canvas>
                { FirstPreview, SecondPreview, ThirdPreview, FourthPreview, FifthPreview, SixthPreview };
        }

        public int GetCanvasIndex(Canvas currentCanvas)
        {
            for (var i = 0; i < allCanvas.Count; i++)
            {
                if (allCanvas[i].Equals(currentCanvas))
                    return i;
            }

            throw new ArgumentException($"canvas with name {currentCanvas.Name} doesnt exist");
        }
    }
}
