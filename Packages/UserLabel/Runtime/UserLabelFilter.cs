using System.Collections.Generic;

namespace GSDev.UserLabel
{
    public interface IUserLabelFilter
    {
        string Name { get; }
        bool CheckPass(IReadonlyUserLabels labels);
    }
    
    public abstract class UserLabelFilter<T> : IUserLabelFilter
    {
        public string Name { get; }
        public string Field { get; }
        
        protected List<T> _targets;
        public IReadOnlyList<T> Targets => _targets;

        public abstract bool CheckPass(IReadonlyUserLabels labels);
        
        public abstract string FilterMethod { get; protected set; }

        protected UserLabelFilter(
            string name, 
            string field,
            IEnumerable<T> targets,
            string filterMethod)
        {
            Name = name;
            Field = field;
            _targets = new List<T>(targets);
            FilterMethod = filterMethod;
        }
    }
}