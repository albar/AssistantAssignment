using System;
using System.Collections.Generic;
using System.Linq;
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2
{
    public class NonDominatedComparer<TKey, TValue> : IComparer<IReadOnlyDictionary<TKey, TValue>>
        where TKey : Enum
        where TValue : IComparable
    {
        private readonly IReadOnlyDictionary<TKey, int> _optimum;

        public NonDominatedComparer(IReadOnlyDictionary<TKey, OptimumValue> optimum)
        {
            _optimum = optimum.ToDictionary(o => o.Key, o => o.Value == OptimumValue.Maximum ? 1 : -1);
        }

        public NonDominatedComparer() :
            this(Enum.GetValues(typeof(TKey)).Cast<TKey>().ToDictionary(k => k, _ => OptimumValue.Maximum))
        {
        }

        public int Compare(IReadOnlyDictionary<TKey, TValue> value1, IReadOnlyDictionary<TKey, TValue> value2)
        {
            if (value1 == null || value2 == null)
            {
                if (value1 == value2) return 0;
                if (value2 != null) return 1;
                return -1;
            }
            
            var domination = 0;
            foreach (var objective in Enum.GetValues(typeof(TKey)).Cast<TKey>())
            {
                var sign = Math.Sign(value1[objective].CompareTo(value2[objective])) * _optimum[objective];
                if (sign == 0 || domination == sign)
                    continue;
                if (domination != 0)
                    return 0;
                domination = sign;
            }

            return domination;
        }
    }
}