using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitPlugin
{
    internal class ProcessingContour
    {
        /// <returns>
        /// Объект ContourFlat2D с данными о контуре комнаты, стенами с дверьми и окнами
        /// </returns>
        public static ContourFlat2D GetContourRoom(Document doc, UIApplication uIApplication)
        {
            var familyInstance = GetSelectedFamilyInstance(doc, uIApplication);
            var contourShape = GetGeometricShape(familyInstance);
            var sideWithDoor = GetSelectedSide(doc, uIApplication, contourShape, "дверью");
            var sideWithWindow = GetSelectedSide(doc, uIApplication, contourShape, "окном");

            return new ContourFlat2D(contourShape, sideWithDoor, sideWithWindow, familyInstance.Name);
        }

        /// <summary>
        /// Запрашивает у пользователя выбор элемента, пока пользователь не выберет элемент типа FamilyInstance
        /// </summary>
        private static FamilyInstance GetSelectedFamilyInstance(Document doc, UIApplication uIApplication)
        {
            while (true)
            {
                var pickedRef = uIApplication.ActiveUIDocument.Selection.PickObject(ObjectType.Subelement,
                    "Выберите контур будущей квартиры");
                var selectedElement = doc.GetElement(pickedRef);
                if (selectedElement is FamilyInstance familyInstance)
                    return familyInstance;
                TaskDialog.Show("Ошибка выбора", $"Вам нужно выбрать 'Модель в контексте выдавливания'\nПопробуйте ещё раз");
            }
        }

        /// <summary>
        /// Запрашивает у пользователя выбор стены, пока пользователь не выберет стену, которая является частью контура и содержит указанный элемент (дверь или окно)
        /// </summary>
        private static Side2D GetSelectedSide(Document doc, UIApplication uIApplication, IGeometricShape2D contourShape, string item)
        {
            while (true)
            {
                var pickedRef = uIApplication.ActiveUIDocument.Selection.PickObject(ObjectType.Subelement,
                    $"Выберите границу(стену) контура с {item}");
                var userClickPoint = pickedRef.GlobalPoint;
                var selectedElement = doc.GetElement(pickedRef);

                if (selectedElement is FamilyInstance familyInstance)
                {
                    var side = contourShape.Sides.Where(s => s.DistanceToPoint(userClickPoint) < 1)
                        .OrderBy(s => s.DistanceToPoint(userClickPoint)).ToArray();
                    if (side.Length > 0)
                        return side[0];
                    TaskDialog.Show("Ошибка выбора", "Укажите точку на границе(стене) контура");
                }
                else
                    TaskDialog.Show("Ошибка выбора", $"Вам нужно выбрать 'Модель в контексте выдавливания'\nПопробуйте ещё раз");
            }
        }

        /// <summary>
        /// Извлекает геометрию элемента, получает его ограничивающую рамку и геометрический экземпляр, извлекает геометрию экземпляра
        /// и твердое тело из нее, получает размеры сторон твердого тела
        /// </summary>
        /// <returns>Новый объект Rectangle2D с этими данными</returns>
        private static IGeometricShape2D GetGeometricShape(FamilyInstance familyInstance)
        {
            var geometry = familyInstance.get_Geometry(new Options());
            var boundingBox = geometry.GetBoundingBox();
            var geometryInstance = GetGeometryInstance(geometry);
            var geometryElement = geometryInstance.GetInstanceGeometry();
            var solid = GetSolid(geometryElement);
            var sizes = GetSizeSides(solid);

            return sizes.Count - 1 == 2 ? new Rectangle2D(boundingBox.Min, boundingBox.Max) : null;
        }

        /// <summary>
        /// Получает геометрический экземпляр из геометрического элемента   
        /// </summary>
        /// <returns>Первый экземпляр GeometryInstance, который метод находит в геометрическом элементе</returns>
        private static GeometryInstance GetGeometryInstance(GeometryElement geometryElement)
        {
            foreach (var element in geometryElement)
                if (element is GeometryInstance geometryInstance)
                    return geometryInstance;

            return null;
        }

        /// <summary>
        /// Получает твердое тело из геометрического элемента
        /// </summary>
        /// <param name="geometryElement"></param>
        /// <returns>Первое твердое тело с положительным объемом, которое этот метод находит в геометрическом элементе</returns>
        private static Solid GetSolid(GeometryElement geometryElement)
        {
            foreach (var element in geometryElement)
            {
                if (!(element is Solid solid)) continue;
                if (solid.Volume > 0)
                    return solid;
            }

            return null;
        }

        /// <summary>
        /// Получает размеры сторон твердого тела - проходит по всем ребрам твердого тела, преобразует их длину в метры,
        /// округляет до трех десятичных знаков и добавляет в список, если такого размера еще нет в списке
        /// </summary>
        private static List<double> GetSizeSides(Solid solid)
        {
            var sizes = new List<double>();

            var edges = solid.Edges;

            foreach (Edge edge in edges)
            {
                var sizeMeter = Math.Round(edge.ApproximateLength * 0.3048, 3);
                if (!sizes.Contains(sizeMeter))
                    sizes.Add(sizeMeter);
            }

            return sizes;
        }
    }
}
