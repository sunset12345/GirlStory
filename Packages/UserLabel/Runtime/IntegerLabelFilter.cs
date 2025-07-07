using System;
using System.Collections.Generic;
using System.Linq;

namespace GSDev.UserLabel
{
    public class IntegerLabelFilter : UserLabelFilter<int>
    {
        public enum CheckMethod : byte
        {
            /// <summary>字段值与目标值相等</summary>
            Equal = 0,
            /// <summary>字段值处于目标数值区间</summary>
            Between,
            /// <summary>字段值大于目标值</summary>
            Greater,
            /// <summary>字段值小于目标值</summary>
            Less
        }
        
        private CheckMethod _checkMethod;
        
        public IntegerLabelFilter(string name, string field, IEnumerable<int> targets, string filterMethod) : 
            base(name, field, targets, filterMethod)
        {
        }

        public override bool CheckPass(IReadonlyUserLabels labels)
        {
            if (!labels.TryGetLabel<int>(Field, out var fieldValue))
                return false;
            if (_targets.Count == 0)
                return false;
            
            switch (_checkMethod)
            {
                case CheckMethod.Equal:
                    return _targets.Any(label => label == fieldValue);
                case CheckMethod.Between:
                    return _targets.Count == 2 && 
                           _targets[0] < fieldValue && 
                           _targets[1] > fieldValue;
                case CheckMethod.Greater:
                    return fieldValue > _targets[0];
                case CheckMethod.Less:
                    return fieldValue < _targets[0];
            }

            return false;
        }

        public override string FilterMethod 
        {
            get => _checkMethod.ToString();
            protected set => Enum.TryParse(value, out _checkMethod);
        }
    }
}