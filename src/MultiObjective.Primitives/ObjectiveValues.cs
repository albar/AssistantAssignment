using System;
using System.Collections;
using System.Collections.Generic;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.Primitives
{
    public struct ObjectiveValues<T> : IReadOnlyDictionary<T, double> where T : Enum
    {
        private readonly IReadOnlyDictionary<T, double> _values;

        public ObjectiveValues(IReadOnlyDictionary<T, double> values)
        {
            _values = values;
        }

        public double this[T key] => _values[key];
        public IEnumerable<T> Keys => _values.Keys;
        public IEnumerable<double> Values => _values.Values;
        public int Count => _values.Count;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<KeyValuePair<T, double>> GetEnumerator() =>_values.GetEnumerator();
        
        public bool ContainsKey(T key) => _values.ContainsKey(key);

        public bool TryGetValue(T key, out double value) =>_values.TryGetValue(key, out value);
    }
}