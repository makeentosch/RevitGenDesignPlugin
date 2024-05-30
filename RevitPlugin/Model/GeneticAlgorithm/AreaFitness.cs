using AreaRoomsAPI.Geometric;
using AreaRoomsAPI.Info;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AreaRoomsAPI.Algorithm
{
    public class AreaFitness : IFitness
    {
        private readonly AreaRoomsFormatsInfo formats;
        private readonly RoomType[] priority;
        private readonly Direction[] directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToArray();
        private readonly int cellWidthCount;
        private readonly int cellHeightCount;

        public AreaFitness(
            AreaRoomsFormatsInfo formats,
            IList<RoomType> roomTypes,
            Dictionary<RoomType, int> roomPriority,
            int cellWidthCount,
            int cellHeightCount)
        {
            this.formats = formats;
            priority = roomTypes.OrderByDescending(x => roomPriority[x]).ToArray();
            this.cellWidthCount = cellWidthCount;
            this.cellHeightCount = cellHeightCount;
        }

        public double Evaluate(IChromosome chromosome)
        {
            var hashSet = new bool[cellHeightCount, cellWidthCount];
            var areaChromosome = (AreaChromosome)chromosome;
            var queue = new Queue<int>();
            var genes = chromosome.GetGenes();
            var roomGenes = new RoomGene[chromosome.Length];

            areaChromosome.ClearCells();
            for (int i = 0; i < chromosome.Length; i++)
            {
                var roomGene = (RoomGene)genes[i].Value;
                areaChromosome.AddCell(i, roomGene.Point);
                roomGene.ClearCells();
                roomGenes[i] = roomGene;
                queue.Enqueue(i);
                hashSet[roomGene.Point.Y, roomGene.Point.X] = true;
            }


            while (queue.Count > 0)
            {
                var geneIndex = queue.Dequeue();
                var gene = roomGenes[geneIndex];
                (var width, var height) = (gene.CellsWidth, gene.CellsHeight);

                foreach (var side in directions.OrderBy(x => (int)x % 2 == 0 ? height : width).ThenBy(x => x))
                {
                    List<Point> result;
                    bool isLegit;
                    if ((int)side % 2 == 0)
                    {
                        isLegit = TryFindWidthCells(gene, side, hashSet, out result);
                    }
                    else
                    {
                        isLegit = TryFindHeightCells(gene, side, hashSet, out result);
                    }

                    if (isLegit)
                    {
                        areaChromosome.AddCells(geneIndex, result);
                        roomGenes[geneIndex].AddCells(result);
                        SetCells(hashSet, result);
                        queue.Enqueue(geneIndex);
                        break;
                    }
                }
            }

            for (var i = 0; i < roomGenes.Length; i++)
            {
                areaChromosome.ReplaceGene(i, new Gene(roomGenes[i]));
            }

            SetToDefault(hashSet);
            return CalculateFitness(areaChromosome, roomGenes);
        }

        private double CalculateFitness(AreaChromosome areaChromosome, RoomGene[] roomGenes)
        {
            var fitness = 0d;
            var diff = roomGenes.Length - roomGenes.Distinct().Count();
            fitness -= diff * 1000;

            for (int i = 0; i < roomGenes.Length; i++)
            {
                var gene = roomGenes[i];
                var roomFormat = formats[priority[i]];
                fitness += CalculateFitnessOfRoom(gene, roomFormat, formats.Ratio);
            }

            areaChromosome.Fitness = fitness;
            return fitness;
        }

        private double CalculateFitnessOfRoom(RoomGene gene, RoomFormat format, double ratio)
        {
            var fitness = 0d;
            var square = gene.Area;
            var minWidth = gene.GetMinWidth();
            var maxWidth = gene.GetMaxWidth();
            var roomRatio = maxWidth / minWidth;

            if (format.RecWidth > 0)
                fitness -= Math.Pow(format.RecWidth - minWidth, 2);
            else if (minWidth < format.MinWidth)
                fitness -= 500 + Math.Pow(format.MinWidth - minWidth, 2);
            else if (minWidth > format.MaxWidth)
                fitness -= 500 + Math.Pow(minWidth - format.MaxWidth, 2);

            if (roomRatio > ratio)
            {
                fitness -= 200 * (roomRatio - ratio);
            }

            if (square < format.MinSquare)
                fitness -= 500 + format.MinSquare - square;
            else if (square > format.MaxSquare)
                fitness -= 500 + square - format.MaxSquare;

            return fitness;
        }

        private void SetToDefault(bool[,] hashSet)
        {
            for (int i = 0; i < cellHeightCount; i++)
            {
                for (int j = 0; j < cellWidthCount; j++)
                {
                    hashSet[i, j] = false;
                }
            }
        }

        private void SetCells(bool[,] hashSet, List<Point> cells)
        {
            foreach (var cell in cells)
            {
                hashSet[cell.Y, cell.X] = true;
            }
        }

        private bool TryFindWidthCells(RoomGene roomGene, Direction direction, bool[,] hashSet, out List<Point> result)
        {
            result = new List<Point>(roomGene.CellsWidth);
            var minWidth = roomGene.minXCell;
            var maxWidth = roomGene.maxXCell + 1;
            var currentPos = direction == Direction.Top ? roomGene.maxYCell + 1 : roomGene.minYCell - 1;
            if (currentPos < 0 || currentPos >= cellHeightCount)
            {
                return false;
            }

            for (int i = minWidth; i < maxWidth; i++)
            {
                var nextPoint = new Point(i, currentPos);
                if (hashSet[nextPoint.Y, nextPoint.X])
                {
                    return false;
                }

                result.Add(nextPoint);
            }

            return true;
        }

        private bool TryFindHeightCells(RoomGene roomGene, Direction direction, bool[,] hashSet, out List<Point> result)
        {
            result = new List<Point>(roomGene.CellsHeight);
            var minHeight = roomGene.minYCell;
            var maxHeight = roomGene.maxYCell + 1;
            var currentPos = direction == Direction.Left ? roomGene.minXCell - 1 : roomGene.maxXCell + 1;
            if (currentPos < 0 || currentPos >= cellWidthCount)
            {
                return false;
            }

            for (int i = minHeight; i < maxHeight; i++)
            {
                var nextPoint = new Point(currentPos, i);
                if (hashSet[nextPoint.Y, nextPoint.X])
                {
                    return false;
                }

                result.Add(nextPoint);
            }

            return true;
        }
    }
}