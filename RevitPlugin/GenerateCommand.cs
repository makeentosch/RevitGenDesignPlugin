using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitPlugin.View;
using System.Linq;

namespace RevitPlugin
{
	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class GenerateCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			var uiapp = commandData.Application;
			var doc = uiapp.ActiveUIDocument.Document;
			var sel = uiapp.ActiveUIDocument.Selection;

			Reference pickedref;
			try
			{
				pickedref = sel.PickObject(ObjectType.Element, new WallSelectionFilter(), "Выберите экземпляр семейства Стены");
			}
			catch (Autodesk.Revit.Exceptions.OperationCanceledException)
			{
				return Result.Cancelled;
			}

			var wall = doc.GetElement(pickedref) as FamilyInstance;
			var geometry = wall.Symbol.get_Geometry(new Options());
			var solid = geometry.Select(t => t as Solid).First(t => t.Volume > 0);
			var face = solid.Faces.Cast<PlanarFace>().First(t => t.FaceNormal.Multiply(-1).IsAlmostEqualTo(XYZ.BasisZ));
			var edges = face.GetEdgesAsCurveLoops().Single();
			//foreach (var line in edges)
			//{
			//	TaskDialog.Show("Длина грани", line.Length.ToString());
			//}

			//var trans = new Transaction(doc);
			//trans.Start("");
			//trans.Commit();

			var mainWindow = new PluginUI();
			var result = mainWindow.ShowDialog() ?? false;

			return result ? Result.Succeeded : Result.Cancelled;	
		}
		/* С помощью фильтра при выделении объекта удалось добиться выбора экземпляра семейста Стены, даже если они в сборке.
         * На случай отмены операции пользователем обернул выделение в try...catch.
         * Итак мы получаем объект FamilyInstance. Информация о геометрии хранится в FamilySymbol (не разобрался что это из себя представляет).
         * В GeometryElement должны получится твёрдые тела (Solid). Почему-то присутствует не одно, а два тела. Второе нулевого объёма.
         * Выбираем грань, которая соответствует полу. Разбираем грань на циклы из кривых. Цикл должен получиться один, так как весь периметр замкнут
         * в один цикл. 
         */
	}
}
