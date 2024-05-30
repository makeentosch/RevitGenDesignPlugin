using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Reinsertions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AreaRoomsAPI.Algorithm
{
    public class AreaReinsertion : ReinsertionBase
    {
        public AreaReinsertion()
            : base(canCollapse: false, canExpand: true)
        {
        }

        protected override IList<IChromosome> PerformSelectChromosomes(IPopulation population,
            IList<IChromosome> offspring, IList<IChromosome> parents)
        {
            var parentsCount = Math.Min(parents.Count, population.MinSize - offspring.Count);
            var chromosome = parents[0];
            var newChromosomesCount =
                Math.Max(population.MinSize - offspring.Count, population.MaxSize - population.MinSize);
            if (parentsCount > 0)
            {
                var list = parents.OrderByDescending(p => p.Fitness).Take(newChromosomesCount).ToList();
                for (int i = 0; i < list.Count; i++)
                    offspring.Add(list[i]);
            }

            for (int i = 0; i < newChromosomesCount; i++)
            {
                offspring.Add(chromosome.CreateNew());
            }

            return offspring;
        }
    }
}