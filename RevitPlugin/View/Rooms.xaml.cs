using Autodesk.Revit.DB;
using RevitPlugin.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using RevitPlugin.ViewModel;

namespace RevitPlugin.View
{
    public partial class Rooms : Window
    {
        private RoomsVM viewModel;

        public Rooms(GeometryObject balconyWall, GeometryObject entranceWall, CurveLoop walls,
            Document document, List<RoomType> rooms, AreaRoomsFormatsInfo roomsFormats)
        {
            InitializeComponent();
            viewModel = new RoomsVM(balconyWall, entranceWall, walls, document, rooms, roomsFormats);
            this.DataContext = viewModel;
        }

        private void CanvasPreview_Click(object sender, RoutedEventArgs e)
        {
            var canvas = (Canvas)sender;
            var currentIndex = viewModel.GetCanvasIndex(canvas);
            viewModel.currentAppartment = currentIndex;
            var mulitpier = viewModel.GetMultipier(RoomCanvas, viewModel.appartmentWidth, viewModel.appartmentHeight);
            var withDelta = viewModel.GetDeltaCoordinates(mulitpier, RoomCanvas.Width, viewModel.appartmentWidth);
            var heightDelta = viewModel.GetDeltaCoordinates(mulitpier, RoomCanvas.Height, viewModel.appartmentHeight);
            RoomCanvas.Children.Clear();
            viewModel.DrawAppartment(RoomCanvas, mulitpier, withDelta, heightDelta, currentIndex);
        }

        private void Generate_Appartment(object sender, RoutedEventArgs e)
        {
            viewModel.CreateAppartment(viewModel.currentAppartment);
        }
    }
}
