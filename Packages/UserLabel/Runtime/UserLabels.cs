using System.Collections.Generic;

namespace GSDev.UserLabel
{
    public interface IReadonlyUserLabels
    {
        IReadOnlyDictionary<string, object> Labels { get; }
        bool TryGetLabel<T>(string field, out T output);
    }
    
    public class UserLabels : IReadonlyUserLabels
    {
        private readonly Dictionary<string, object> _labels = new Dictionary<string, object>();
        public IReadOnlyDictionary<string, object> Labels => _labels;

        public bool TryGetLabel<T>(string field, out T output)
        {
            if (_labels.TryGetValue(field, out var value) &&
                value is T casted)
            {
                output = casted;
                return true;
            }
            else
            {
                output = default;
                return false;
            }
        }

        public object GetLabel(string field)
        {
            return _labels.TryGetValue(field, out var output) ?
                output :
                default;
        }

        internal void SetValue(string field, object value)
        {
            _labels[field] = value;
        }

        internal void Clear()
        {
            _labels.Clear();
        }
    }
}