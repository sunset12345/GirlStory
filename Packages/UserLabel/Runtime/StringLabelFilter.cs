using System;
using System.Collections.Generic;
using System.Linq;

namespace GSDev.UserLabel
{
    public class StringLabelFilter : UserLabelFilter<string>
    {
        public enum CheckMethod : byte
        {
            /// <summary>字串全量匹配</summary>
            Full = 0, 
            /// <summary>字段值包含目标值</summary>
            LabelContainTarget,
            /// <summary>目标值包含字段值</summary>
            TargetContainLabel,
            /// <summary>字段值以目标值为开头</summary>
            LabelStartWithTarget,
            /// <summary>目标值以字段值为开头</summary>
            TargetStartWithLabel
        }
        
        private CheckMethod _checkMethod;
        
        public StringLabelFilter(string name, string field, IEnumerable<string> targets, string filterMethod) : 
            base(name, field, targets, filterMethod)
        {
        }

        public override bool CheckPass(IReadonlyUserLabels labels)
        {
            if (!labels.TryGetLabel<string>(Field, out var fieldValue))
                return false;
            if (string.IsNullOrEmpty(fieldValue))
                return false;

            const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;

            switch (_checkMethod)
            {
                case CheckMethod.Full:
                    return _targets.Any(label => 
                        label.Equals(fieldValue, comparison));
                case CheckMethod.LabelContainTarget:
                    return _targets.Any(label => 
                        fieldValue.IndexOf(label, comparison) >= 0);
                case CheckMethod.TargetContainLabel:
                    return _targets.Any(label =>
                        label.IndexOf(fieldValue, comparison) >= 0);
                case CheckMethod.LabelStartWithTarget:
                    return _targets.Any(label => 
                        fieldValue.StartsWith(label, comparison));
                case CheckMethod.TargetStartWithLabel:
                    return _targets.Any(label =>
                        label.StartsWith(fieldValue, comparison));
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