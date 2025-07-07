using System.Collections.Generic;

namespace GSDev.UserLabel
{
    public class UserGroup
    {
        private List<string> _includedFilters = new List<string>();
        internal IReadOnlyList<string> IncludedFilters => _includedFilters;
        private List<string> _excludedFilters = new List<string>();
        internal IReadOnlyList<string> ExcludedFilters => _excludedFilters;
        
        public int Priority { get; set; } = 0;
        public string RuleName { get; set; }

        public void AddIncludedFilter(string filter)
        {
            _includedFilters.Add(filter);
        }

        public void AddExcludedFilter(string filter)
        {
            _excludedFilters.Add(filter);
        }
    }
}