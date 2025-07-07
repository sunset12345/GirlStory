using System.Collections.Generic;

namespace GSDev.UserLabel
{
    public class UserRule
    {
        public string Name { get; }

        public UserRule(string name)
        {
            Name = name;
        }
        
        private Dictionary<string, string> _stringReplacements = new Dictionary<string, string>();

        public void AddReplacement(string origin, string replace)
        {
            _stringReplacements.Add(origin, replace);
        }

        public string GetReplacement(string origin)
        {
            return _stringReplacements.TryGetValue(origin, out var replace) ? 
                replace : 
                null;
        }
    }
}