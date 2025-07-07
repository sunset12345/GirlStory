using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GSDev.UserLabel
{
    public class UserLabelManager
    {
        public static UserLabelManager Instance { get; } = new UserLabelManager();

        private readonly Dictionary<string, IUserLabelFilter> _filters = new Dictionary<string, IUserLabelFilter>();
        public IReadOnlyDictionary<string, IUserLabelFilter> Filters => _filters;
        
        private readonly Dictionary<string, UserRule> _rules = new Dictionary<string, UserRule>();
        public IReadOnlyDictionary<string, UserRule> Rules => _rules;
        
        private readonly List<UserGroup> _groups = new List<UserGroup>();
        public IReadOnlyList<UserGroup> Groups => _groups;
        
        private readonly UserLabels _labels = new UserLabels();
        public IReadOnlyDictionary<string, object> Labels => _labels.Labels;

        private UserRule _matchedRule = null;
        public UserRule MatchedRule => _matchedRule;

        /// <summary>
        /// 获取全量标签
        /// </summary>
        /// <param name="methodInfos">标签获取方法的MethodInfo, 必须是static</param>
        public void FetchLabels(IEnumerable<MethodInfo> methodInfos)
        {
            foreach (var info in methodInfos)
            {
                var attribute = info.GetCustomAttribute<UserLabelGetterAttribute>();
                if (attribute == null) 
                    continue;
                
                var value = info.Invoke(null, null);
                _labels.SetValue(attribute.Field, value);
            }
        }

        public bool CheckMatch(
            IEnumerable<string> includeFilters, 
            IEnumerable<string> excludeFilters)
        {
            try
            {
                return (includeFilters == null || 
                       includeFilters.Select(name => _filters[name])
                           .All(filter => filter.CheckPass(_labels))) &&
                       (excludeFilters == null ||
                       excludeFilters.Select(name => _filters[name])
                           .All(filter => !filter.CheckPass(_labels)));
            }
            catch
            {
                return false;
            }
        }

        public void RefreshMatchedRule()
        {
            var matches = _groups.FindAll(group => 
                CheckMatch(
                    group.IncludedFilters, 
                    group.ExcludedFilters));
            
            _matchedRule = null;
            if (matches.Count == 0) 
                return;
            var userGroup = matches[0];
            if (matches.Count > 1)
            {
                matches.Sort((a, b) => a.Priority - b.Priority);
                userGroup = matches[matches.Count - 1];
            }

            if (_rules.TryGetValue(userGroup.RuleName, out var rule)) 
                _matchedRule = rule;
        }
        
        public T GetFieldValue<T>(string field)
        {
            return _labels.TryGetLabel<T>(field, out var value) ?
                value :
                default;
        }

        public object GetFieldValue(string field)
        {
            return _labels.GetLabel(field);
        }

        public void AddFilter(IUserLabelFilter filter)
        {
            _filters.Add(filter.Name, filter);
        }

        public void AddRule(UserRule rule)
        {
            _rules.Add(rule.Name, rule);
        }
        
        public void AddGroup(UserGroup matcher)
        {
            _groups.Add(matcher);
        }

        public void Clear() 
        {
            _labels.Clear();
            _filters.Clear();
            _rules.Clear();
            _groups.Clear();
        }
        
        public string GetReplacement(string origin)
        {
            if (_matchedRule == null) 
                return origin;
            var replace = _matchedRule.GetReplacement(origin);
            return replace ?? origin;
        }
    }
}

