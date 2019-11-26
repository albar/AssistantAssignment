using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Thesis.DataType
{
    public enum Assesments
    {
        BasicKnowledge,
        LearningAbility,
        PublicSpeaking,
        Hospitality,
    }

    public static class AssesmentsExtensions
    {
        public static readonly IEnumerable<Assesments> AllAssessments =
            Enum.GetValues(typeof(Assesments)).Cast<Assesments>().ToImmutableArray();

        public static readonly IComparer<ImmutableDictionary<Assesments, double>> Comparer =
            new AssesmentsComparer();

        private class AssesmentsComparer : IComparer<ImmutableDictionary<Assesments, double>>
        {
            public int Compare(ImmutableDictionary<Assesments, double> x,
                ImmutableDictionary<Assesments, double> y)
            {
                var sign = 0;
                foreach (var assesment in AllAssessments)
                {
                    var lsign = x[assesment].CompareTo(y[assesment]);
                    if (lsign == 0 || sign != 0 && sign != lsign) return 0;
                    sign = lsign;
                }

                return sign;
            }
        }
    }
}
