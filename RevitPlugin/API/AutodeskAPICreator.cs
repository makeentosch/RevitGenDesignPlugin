using AreaRoomsAPI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using RevitPlugin.Geometry;

namespace RevitPlugin.API
{
    public static class AutodeskAPICreator
    {
        public static List<Curve> GetCurvesByPoints(IList<PointD> points, Document document)
        {
            var result = new List<Curve>();
            var height = document.ActiveView.GenLevel.ProjectElevation;
            for (var i = 0; i < points.Count; i++)
            {
                var line = points[i];
                var nextLine = points[(i + 1) % points.Count];
                var startPoint = new XYZ(line.X, line.Y, height);
                var endPoint = new XYZ(nextLine.X, nextLine.Y, height);
                result.Add(Line.CreateBound(startPoint, endPoint));
            }

            return result;
        }

        public static void CreateRooms(List<List<Curve>> allCurves, Document document)
        {
            var assemblyElements = new List<ElementId>();
            var assemblyElementsId = new ElementId(-2000011);

            using (var transaction = new Transaction(document, "Create room"))
            {
                transaction.Start();
                foreach (var curves in allCurves)
                {
                    var height = GetLevelHeight(document.ActiveView.GenLevel, GetAllLevels(document));
                    var shape = DirectShape.CreateElement(document, new ElementId(-2000011));
                    var loop = CurveLoop.Create(curves);
                    var options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

                    var room = GeometryCreationUtilities
                        .CreateExtrusionGeometry(
                            new CurveLoop[] { loop },
                            XYZ.BasisZ, height, options);

                    shape.SetShape(new List<GeometryObject> { room });
                    shape.SetName($"Room");
                    assemblyElements.Add(shape.Id);
                }

                transaction.Commit();
            }

            CreateAssembly(document, assemblyElements, assemblyElementsId);
        }

        public static void CreateAssembly(Document document, List<ElementId> assemblyElements,
            ElementId assemblyElementsId)
        {
            using (var transaction = new Transaction(document, "Create Assembly"))
            {
                transaction.Start();

                if (!AssemblyInstance.IsValidNamingCategory(document, assemblyElementsId, assemblyElements))
                {
                    throw new ArgumentException("Невозможно создать такую сборку");
                }

                AssemblyInstance.Create(document, assemblyElements, assemblyElementsId);

                transaction.Commit();
            }
        }

        private static double GetLevelHeight(Level currentLevel, IList<Level> allLevels)
        {
            var height = 0.0;

            if (allLevels.Count == 0)
            {
                throw new ArgumentException("Должен быть добавлен как минимум один этаж");
            }

            if (allLevels[0].Id == currentLevel.Id)
            {
                height = currentLevel.Elevation;
            }

            else
            {
                for (var i = 0; i < allLevels.Count; i++)
                {
                    if (allLevels[i].Id == currentLevel.Id)
                    {
                        height = currentLevel.Elevation - allLevels[i - 1].Elevation;
                    }
                }
            }

            return height;
        }

        private static IList<Level> GetAllLevels(Document document)
        {
            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Level))
                .Select(el => el as Level)
                .ToList();
        }
    }
}