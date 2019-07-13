using System;
using System.Collections.Generic;
using System.Linq;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2
{
    public class NonDominatedComparer<TKey, TValue> : IComparer<IReadOnlyDictionary<TKey, TValue>>
        where TKey : Enum
        where TValue : IComparable
    {
        private readonly IReadOnlyDictionary<TKey, double> _coefficients;

        public NonDominatedComparer(IReadOnlyDictionary<TKey, double> coefficients)
        {
            _coefficients = coefficients;
        }

        public NonDominatedComparer() :
            this(Enum.GetValues(typeof(TKey)).Cast<TKey>().ToDictionary(k => k, _ => 1d))
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
            foreach (var optimum in _coefficients)
            {
                var sign = Math.Sign(value1[optimum.Key].CompareTo(value2[optimum.Key]) * optimum.Value);
                if (sign == 0 || domination != 0 && domination != sign) return 0;
                if (domination == sign) continue;
                domination = sign;
            }

            return domination;
        }
    }
}