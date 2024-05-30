using AreaRoomsAPI;
using AreaRoomsAPI.Info;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CommunityToolkit.Mvvm.Input;
using RevitPlugin.Model;
using RevitPlugin.View;
using RevitPlugin.View.Pages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RevitPlugin.ViewModel
{
	public class CommandSetupVM
	{
		private UIApplication revitApp;
		private PluginUI ui;
		private List<Curve> perimeter;
		private GeneratedArea generatedArea;

		public Result? CommandResult { get; private set; }
		public GeneratedArea SelectedGenArea
		{
			get => generatedArea;
			set
			{
				generatedArea = value;
				var canvas = ((SelectLayout)Application.Current.MainWindow.Content).Image;
				canvas.Children.Add(new System.Windows.Shapes.Rectangle() { RadiusX = 40, RadiusY = 40 });
			}
		}
		public List<GeneratedArea> GeneratedAreas { get; private set; }

		public List<RoomParameters> LayoutParameters { get; } = new List<RoomParameters>
		{
			new RoomParameters("Кухня", 2.8, 10, 0.5, RoomType.Kitchen),
			new RoomParameters("Коридор", 1.2, 5, 0.5, RoomType.Corridor),
			new RoomParameters("Ванная", 1.7, 4, 0.5, RoomType.Toilet)
		};
		public RelayCommand NextPage { get => new RelayCommand(GoToNextPage); }
		public RelayCommand PreviousPage { get => new RelayCommand(GoToPreviousPage); }
		public RelayCommand Cancel { get => new RelayCommand(CancelingOperation); }

		public CommandSetupVM(UIApplication revitApp)
		{
			this.revitApp = revitApp;
		}

		public void Start()
		{
			SavePerimeter();
			if (CommandResult != null) return;
			ui = new PluginUI() { DataContext = this };
			GoToPreviousPage();
			CommandResult = ui.ShowDialog() ?? false ? Result.Succeeded : Result.Failed;
		}

		private void SavePerimeter()
		{
			var doc = revitApp.ActiveUIDocument.Document;
			var sel = revitApp.ActiveUIDocument.Selection;

			Reference pickedref;
			try
			{
				pickedref = sel.PickObject(ObjectType.Element, new WallSelectionFilter(), "Выберите экземпляр семейства Стены");
			}
			catch (Autodesk.Revit.Exceptions.OperationCanceledException)
			{
				CommandResult = Result.Cancelled;
				return;
			}

			var wall = doc.GetElement(pickedref) as FamilyInstance;
			var geometry = wall.Symbol.get_Geometry(new Options());
			var solid = geometry.Select(t => t as Solid).First(t => t.Volume > 0);
			var face = solid.Faces.Cast<PlanarFace>().First(t => t.FaceNormal.Multiply(-1).IsAlmostEqualTo(XYZ.BasisZ));
			perimeter = face.GetEdgesAsCurveLoops().Single().ToList();
		}

		private void GoToNextPage()
		{
			var currentPage = ui.mainFrame.Content;
			Page nextPage;
			if (currentPage is LayoutParameters)
			{
				nextPage = new GeneratorParameters();
			}
			else if (currentPage is GeneratorParameters)
			{
				var areaInfo = new AreaInfo(
				TransformData.TransformAutodeskWallsToApi(perimeter[0], perimeter[1], perimeter, new XYZ(), new XYZ()),
				0.0, LayoutParameters.Select(t => t.RoomType).ToList());

				var generator = new RoomsGenerator(
					areaInfo,
					new AreaRoomsFormatsInfo(LayoutParameters.ToDictionary(t => t.RoomType, t => new RoomFormat(minWidth: t.Width, minSquare: t.Area))));
				GeneratedAreas = generator.GenerateAreas(LayoutParameters.Count);



				nextPage = new SelectLayout();//new WaitFinish();
			}
			else throw new Exception("Unexpexted page in main frame");

			nextPage.DataContext = this;
			ui.mainFrame.Navigate(nextPage);
		}

		private void GoToPreviousPage()
		{
			var currentPage = ui.mainFrame.Content;
			Page nextPage;
			if (currentPage is SelectLayout || currentPage is WaitFinish)
			{
				nextPage = new GeneratorParameters();
			}
			else if (currentPage is GeneratorParameters || currentPage is null)
			{
				nextPage = new LayoutParameters();
				((LayoutParameters)nextPage).table.InitializingNewItem += (sender, args) =>
				{
					((RoomParameters)args.NewItem).Name = $"Комната {LayoutParameters.Count - 3}";
				};
			}
			else throw new Exception("Unexpexted page in main frame");

			nextPage.DataContext = this;
			ui.mainFrame.Navigate(nextPage);
		}

		private void CancelingOperation()
		{
			ui.DialogResult = false;
			ui.Close();
		}

		private void ProgressBarValueChange(double value)
		{
			var waitPage = ui.mainFrame.Content as WaitFinish;
			var progressBar = waitPage.FindName("progressBar") as ProgressBar;
			progressBar.Value = value;
			if (progressBar.Value == progressBar.Maximum)
			{
				ui.mainFrame.Navigate(new SelectLayout() { DataContext = this });
			}
		}
	}
}
