using RevitPlugin.ViewModel;
using Autodesk.Revit.DB;
using System.Windows;

namespace RevitPlugin.View
{
    public partial class ApartmentParams : Window
    {
        private ApartmentParamsVM viewModel;

        public ApartmentParams(GeometryObject balconyWall, GeometryObject entranceWall, CurveLoop walls, Document document)
        {
            InitializeComponent();
            viewModel = new ApartmentParamsVM(balconyWall, entranceWall, walls, document);
            DataContext = viewModel;
        }

        private void Generate_Room(object sender, RoutedEventArgs e)
        {
            viewModel.GenerateRoom(sender, e);
        }
    }
}
