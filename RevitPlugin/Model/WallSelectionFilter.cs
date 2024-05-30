using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitPlugin
{
	public class WallSelectionFilter : ISelectionFilter
	{
		public bool AllowElement(Element elem)
		{
			return elem is FamilyInstance family
				&& family.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls;
		}

		public bool AllowReference(Reference reference, XYZ position) => true;
	}
}
