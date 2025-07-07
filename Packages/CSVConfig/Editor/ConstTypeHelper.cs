using System;
using System.Text;
using System.Text.RegularExpressions;

namespace GSDev.CSVConfig.Editor
{
    internal abstract class ConstTypeHelper
    {
        protected const string DataGetterName = "raw";

        protected string _fieldName;
        protected string PropertyName => $"__{_fieldName}";
        
        internal abstract string TypeName { get; }
        
        internal ConstTypeHelper(string fieldName)
        {
            _fieldName = fieldName;
        }

        public virtual void Check(string data, string fileName)
        {
            if (data.Length == 0)
            {
                throw new System.Exception($"[{fileName}:{_fieldName}] 字段不允许为空");
            }
        }

        public virtual string GenerateReader()
        {
            return $@"
        if (rawData.TryGetValue(""{_fieldName}"", out {DataGetterName}))
        {{
            {PropertyName} = {GenerateFieldParser(DataGetterName)};
        }}"; 
        }

        public virtual string GenerateProperty()
        {
            var output = $@"
    public {TypeName} {_fieldName} => {PropertyName};";
            output += $@"
    private {TypeName} {PropertyName};";
            return output;
        }

        public virtual string GenerateFieldParser(string content)
        {
            return $"{TypeName}.Parse({content})";
        }
    }


    internal static class ConstTypeHelperFactory
    {
        public static ConstTypeHelper CreateHelper(string typeDescription, string fieldName)
        {
            if (TypeMatchRegex.SbyteRegex.IsMatch(typeDescription))
                return new ConstTypeHelperSByte(fieldName);
            if (TypeMatchRegex.ByteRegex.IsMatch(typeDescription))
                return new ConstTypeHelperByte(fieldName);
            if (TypeMatchRegex.ShortRegex.IsMatch(typeDescription))
                return new ConstTypeHelperShort(fieldName);
            if (TypeMatchRegex.UshortRegex.IsMatch(typeDescription))
                return new ConstTypeHelperUShort(fieldName);
            if (TypeMatchRegex.IntRegex.IsMatch(typeDescription))
                return new ConstTypeHelperInt(fieldName);
            if (TypeMatchRegex.UintRegex.IsMatch(typeDescription))
                return new ConstTypeHelperUInt(fieldName);
            if (TypeMatchRegex.LongRegex.IsMatch(typeDescription))
                return new ConstTypeHelperLong(fieldName);
            if (TypeMatchRegex.UlongRegex.IsMatch(typeDescription))
                return new ConstTypeHelperULong(fieldName);
            if (TypeMatchRegex.FloatRegex.IsMatch(typeDescription))
                return new ConstTypeHelperFloat(fieldName);
            if (TypeMatchRegex.BoolRegex.IsMatch(typeDescription))
                return new ConstTypeHelperBool(fieldName);
            if (TypeMatchRegex.StringRegex.IsMatch(typeDescription))
                return new ConstTypeHelperString(fieldName);
            if (TypeMatchRegex.ListRegex.IsMatch(typeDescription))
                return new ConstTypeHelperList(fieldName, typeDescription);
            if (TypeMatchRegex.DictRegex.IsMatch(typeDescription))
                return new ConstTypeHelperDictionary(fieldName, typeDescription);
            if (TypeMatchRegex.ClassRegex.IsMatch(typeDescription))
                return new ConstTypeHelperClass(fieldName, typeDescription);
            return null;
        }
    }

    internal class ConstTypeHelperSByte : ConstTypeHelper
    {
        public ConstTypeHelperSByte(string fieldName) : base(fieldName)
        {
        }

        internal override string TypeName => "sbyte";
    }

    internal class ConstTypeHelperByte : ConstTypeHelper
    {
        public ConstTypeHelperByte(string fieldName) : base(fieldName)
        {
        }

        internal override string TypeName => "byte";
    }
    
    internal class ConstTypeHelperShort : ConstTypeHelper
    {
        public ConstTypeHelperShort(string fieldName) : base(fieldName)
        {
        }

        internal override string TypeName => "short";
    }
    
    internal class ConstTypeHelperUShort : ConstTypeHelper
    {
        public ConstTypeHelperUShort(string fieldName) : base(fieldName)
        {
        }

        internal override string TypeName => "ushort";
    }
    
    internal class ConstTypeHelperInt : ConstTypeHelper
    {
        public ConstTypeHelperInt(string fieldName) : base(fieldName)
        {
        }

        internal override string TypeName => "int";
    }
    
    internal class ConstTypeHelperUInt : ConstTypeHelper
    {
        public ConstTypeHelperUInt(string fieldName) : base(fieldName)
        {
        }

        internal override string TypeName => "uint";
    }
    
    internal class ConstTypeHelperLong : ConstTypeHelper
    {
        public ConstTypeHelperLong(string fieldName) : base(fieldName)
        {
        }

        internal override string TypeName => "long";
    }
    
    internal class ConstTypeHelperULong : ConstTypeHelper
    {
        public ConstTypeHelperULong(string fieldName) : base(fieldName)
        {
        }

        internal override string TypeName => "ulong";
    }
    
    internal class ConstTypeHelperFloat : ConstTypeHelper
    {
        public ConstTypeHelperFloat(string fieldName) : base(fieldName)
        {
        }

        internal override string TypeName => "float";
        
        public override string GenerateFieldParser(string content)
        {
            return $"{TypeName}.Parse({content}, System.Globalization.CultureInfo.InvariantCulture)";
        }
    }

    internal class ConstTypeHelperBool : ConstTypeHelper
    {
        public ConstTypeHelperBool(string fieldName) : base(fieldName)
        {
        }

        internal override string TypeName => "bool";

        public override string GenerateFieldParser(string content)
        {
            return $"string.Equals(bool.TrueString, {content}, System.StringComparison.InvariantCultureIgnoreCase)";
        }
    }
    
    internal class ConstTypeHelperString : ConstTypeHelper
    {
        public ConstTypeHelperString(string fieldName) : base(fieldName)
        {
        }

        internal override string TypeName => "string";
        
        public override string GenerateFieldParser(string content)
        {
            return content;
        }
    }

    internal class ConstTypeHelperList : ConstTypeHelper
    {
        private string _typeDescription;
        private ConstTypeHelper _subTypeHelper;
        public ConstTypeHelperList(string fieldName, string typeDescription) : base(fieldName)
        {
            _typeDescription = typeDescription;
            var tmp = _typeDescription.Replace(">", "");
            var ipos = tmp.IndexOf("<");
            tmp = tmp.Substring(ipos + 1);
            
            _subTypeHelper = ConstTypeHelperFactory.CreateHelper(tmp, fieldName);
        }

        internal override string TypeName => $"List<{_subTypeHelper.TypeName}>";

        public override void Check(string data, string fileName)
        {
            base.Check(data, fileName);
            var fields = data.Split(new[] {FlagChar.ListSeparator});
            foreach (var d in fields)
                _subTypeHelper.Check(d, fileName);
        }

        public override string GenerateProperty()
        {
            var output = $@"
    public {TypeName} {_fieldName} => {PropertyName};";
            output += $@"
    private readonly {TypeName} {PropertyName} = new {TypeName}();";
            return output;
        }

        public override string GenerateReader()
        {
            var builder = new StringBuilder();
            builder.Append($@"
        if (rawData.TryGetValue(""{_fieldName}"", out {DataGetterName}))
        {{");

            builder.Append($@"
            var fields = {DataGetterName}.Split(new string[]{{""{FlagChar.ListSeparator}""}}, System.StringSplitOptions.RemoveEmptyEntries);");
            builder.Append($@"
            {PropertyName}.Clear();
            foreach (var f in fields)
            {{
                if (string.IsNullOrEmpty(f))
                    continue;
                {PropertyName}.Add({_subTypeHelper.GenerateFieldParser("f")});
            }}
        }}");

            return builder.ToString();
        }

        public override string GenerateFieldParser(string content)
        {
            throw new Exception($"[Const] List's field parser called in field type:{_fieldName}!");
        }
    }

    internal class ConstTypeHelperDictionary : ConstTypeHelper
    {
        private string _typeDescription;
        private ConstTypeHelper _keyTypeHelper;
        private ConstTypeHelper _valueTypeHelper;
        
        public ConstTypeHelperDictionary(string fieldName, string typeDescription) : base(fieldName)
        {
            _typeDescription = typeDescription;
            var tail = new Regex(">$");
            
            var tmp = tail.Replace( typeDescription, "");
            var headPos = tmp.IndexOf("<");
            tmp = tmp.Substring(headPos + 1);

            var kv = tmp.Split('&');
            
            _keyTypeHelper = ConstTypeHelperFactory.CreateHelper(kv[0], fieldName);
            _valueTypeHelper = ConstTypeHelperFactory.CreateHelper(kv[1], fieldName);
        }

        internal override string TypeName => $"Dictionary<{_keyTypeHelper.TypeName}, {_valueTypeHelper.TypeName}>";

        public override void Check(string data, string fileName)
        {
            base.Check(data, fileName);
            var keyValues = data.Split(FlagChar.DictionarySeparator);
            foreach (var kv in keyValues)
            {
                var split = kv.Split(FlagChar.KeyValueSeparator);
                if (split.Length != 2)
                    throw new Exception($"[{fileName}:{_fieldName}] Key Value Error!");
                _keyTypeHelper.Check(split[0], fileName);
                _valueTypeHelper.Check(split[1], fileName);
            }
        }

        public override string GenerateProperty()
        {
            var output = $@"
    public {TypeName} {_fieldName} => {PropertyName};";
            output += $@"
    private readonly {TypeName} {PropertyName} = new {TypeName}();";
            return output;
        }

        public override string GenerateReader()
        {
            var builder = new StringBuilder();
            builder.Append($@"
        if (rawData.TryGetValue(""{_fieldName}"", out {DataGetterName}))
        {{");

            builder.Append($@"
            var keyValues = {DataGetterName}.Split(new string[]{{""{FlagChar.DictionarySeparator}""}}, System.StringSplitOptions.RemoveEmptyEntries);");
            builder.Append($@"
            {PropertyName}.Clear();
            foreach (var kv in keyValues)
            {{
                if (string.IsNullOrEmpty(kv))
                    continue;
                var split = kv.Split('{FlagChar.KeyValueSeparator}');
                if (split.Length != 2)
                    continue;");
            builder.Append($@"
                var k = {_keyTypeHelper.GenerateFieldParser("split[0]")};");
            builder.Append($@"
                var v = {_valueTypeHelper.GenerateFieldParser("split[1]")};");
            builder.Append($@"
                {PropertyName}.Add(k, v);
            }}
        }}");

            return builder.ToString();
        }

        public override string GenerateFieldParser(string content)
        {
            throw new Exception($"[Const] Dictionary's field parser called in field type:{_fieldName}!");
        }
    }

    internal class ConstTypeHelperClass : ConstTypeHelper
    {
        private readonly string _typeName;
        public ConstTypeHelperClass(string fieldName, string typeDescription) : base(fieldName)
        {
            var vvc = typeDescription.Split(new string[] { "@" }, System.StringSplitOptions.None);
            _typeName = vvc[1];
        }

        internal override string TypeName => _typeName;
        
        public override string GenerateFieldParser(string content)
        {
            return $"{TypeName}.Parse({content})";
        }
    }
}