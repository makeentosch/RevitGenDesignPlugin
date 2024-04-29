//using GeneticSharp.Domain.Chromosomes;
//using GeneticSharp.Domain.Populations;
//using GeneticSharp.Domain.Randomizations;
//using GeneticSharp.Domain.Selections;
//using GeneticSharp.Infrastructure.Framework.Texts;
//using System.Collections.Generic;
//using System.Linq;

//namespace AreaRoomsAPI.Algorithm
//{
//    public class AreaTournamentSelection : SelectionBase
//    {
//        public int Size { get; set; }

//        public AreaTournamentSelection() : this(2)
//        {
//        }

//        public AreaTournamentSelection(int size) : base(2)
//        {
//            Size = size;
//        }

//        protected override IList<IChromosome> PerformSelectChromosomes(int number, Generation generation)
//        {
//            number = number / 2;

//            if (Size > generation.Chromosomes.Count)
//            {
//                throw new SelectionException(this,
//                    "The tournament size is greater than available chromosomes. Tournament size is {0} and generation {1} available chromosomes are {2}."
//                        .With(Size, generation.Number, generation.Chromosomes.Count));
//            }

//            List<IChromosome> list = generation.Chromosomes.ToList();
//            List<IChromosome> list2 = new List<IChromosome>();
//            while (list2.Count < number)
//            {
//                int[] randomIndexes = RandomizationProvider.Current.GetUniqueInts(Size, 0, list.Count);
//                IChromosome item = GetChromosomes(list, randomIndexes).OrderByDescending(x => x.Fitness).First();
//                list2.Add(item);

//                list.Remove(item);
//            }

//            return list2;
//        }

//        private IEnumerable<IChromosome> GetChromosomes(IList<IChromosome> chromosomes, int[] randomIndexes)
//        {
//            foreach (var index in randomIndexes)
//                yield return chromosomes[index];
//        }
//    }
//}