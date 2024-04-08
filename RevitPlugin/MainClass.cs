using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;

namespace RevitGenDesignPlugin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MainClass : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var doc = uiapp.ActiveUIDocument.Document;

            var sel = uiapp.ActiveUIDocument.Selection;
            var pickedref = sel.PickObject(ObjectType.Element, "Выберите объект из категории стены"); // фильтр?
            var elem = doc.GetElement(pickedref);
            if (elem is AssemblyInstance assemblyInstance)
            {
                var memIds = assemblyInstance.GetMemberIds();
                if (memIds.Count != 1) return Result.Failed;
                elem = doc.GetElement(memIds.First());
            }

			var shapes = elem.GetGeometryObjectFromReference(new Reference(elem)) as GeometryElement;
            foreach (var geometryObject in shapes)
            {
                if (geometryObject is Face face)
                {
                    var curves = face.GetEdgesAsCurveLoops();
                }
            }

            //var trans = new Transaction(doc);
            //trans.Start("");
            //trans.Commit();

            return Result.Succeeded;
        }
    }
}