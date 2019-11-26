using System;
using System.Collections;
using System.Collections.Generic;

namespace Thesis.Algorithm
{
    public class ObjectivesValue : IReadOnlyDictionary<Objectives, double>, IComparable<ObjectivesValue>
    {
        public static readonly IComparer<ObjectivesValue> ObjectivesValueComparer =
            new Comparer();

        private readonly IReadOnlyDictionary<Objectives, double> _values;

        public ObjectivesValue(IReadOnlyDictionary<Objectives, double> objectiveValues)
        {
            _values = objectiveValues;
        }

        public double this[Objectives key] => _values[key];
        public IEnumerable<Objectives> Keys => _values.Keys;

        public IEnumerable<double> Values => _values.Values;
        public int Count => _values.Count;

        public int CompareTo(ObjectivesValue other)
        {
            return ObjectivesValueComparer.Compare(this, other);
        }

        public bool ContainsKey(Objectives key)
        {
            return _values.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<Objectives, double>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public bool TryGetValue(Objectives key, out double value)
        {
            return _values.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Comparer : IComparer<ObjectivesValue>
        {
            public int Compare(ObjectivesValue x, ObjectivesValue y)
            {
                var sign = 0;
                foreach (var key in x.Keys)
                {
                    var lsign = x[key].CompareTo(y[key]);
                    if (lsign == 0 || sign != 0 && sign != lsign) return 0;
                    sign = lsign;
                }

                return sign;
            }
        }
    }
}
