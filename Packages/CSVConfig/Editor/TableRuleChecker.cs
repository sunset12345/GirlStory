using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GSDev.CSVConfig.Editor
{
    public interface ITableChecker
    {
       bool Initialize( string desc );
       void Check( string data, string file, string fieldName, int line );
        string GenerateRowData( string n );
        string GenerateDesc( string fileName, string content, string dst );
    }
    internal class TableNotEmptyChecker : ITableChecker
    {
        public bool Initialize( string desc )
        { return true; }
        public void Check(string data, string file, string fieldName, int line)
        {
            if (data.Length == 0)
            {
                throw new System.Exception($"[{file}:{line}] {fieldName} 字段不允许为空");
            }
        }
        public string GenerateRowData( string n) { return ""; }
        public string GenerateDesc( string fileName, string content, string dst ) { return ""; }
    }

    internal class TableUniqueChecker : ITableChecker
    {
        private readonly List<string> _dList = new List<string>();
        private readonly TableNotEmptyChecker _empty = new TableNotEmptyChecker();
        public bool Initialize(string desc)
        {
            return _empty.Initialize(desc);
        }
        public void Check(string data, string file, string fieldName, int line)
        {
            _empty.Check(data, file, fieldName, line);

            if (data.Length == 0) return;

            foreach (var v in _dList)
            {
                if (v.Equals(data))
                {
                    throw new System.Exception($"[{file}:{line}] {fieldName}字段值{data}重复");
                }
            }

            _dList.Add(data);

        }
        public string GenerateRowData( string n ) { return ""; }
        public string GenerateDesc(string fileName, string content, string dst) { return ""; }
       
    }
    internal interface ITableTypeRuleHelper
    {
        string GetTypeName();
        void Check(string data, string file, string fieldName, int line);
        string Generate( string n );
        string GenerateDesc( string fieldName, string content, string dst );
        string GenerateParse(string fieldName, string content, string dst);
    }

    //class Type_RuleHelper_Unkonwn : Type_RuleHelper
    //{
    //    public string GetTypeName() { return ""; }
    //    public void Check( string data, string file, string filed_name, int line )
    //    { }

    //    public string Generate( string n )
    //    { return ""; }

    //    public string GenerateDesc(string fieldName, string content, string dst)
    //    {
    //        return "";
    //    }
    //    public string GenerateParse(string fieldName, string content, string dst)
    //    {
    //        return "";
    //    }

    //}
    
    internal class TableTypeRuleHelperClass: ITableTypeRuleHelper
    {
        private readonly string _type;
        public TableTypeRuleHelperClass(string typeDesc)
        {
            var vvc = typeDesc.Split(new string[] { "@" }, System.StringSplitOptions.None);
            _type = vvc[1];
        }
        public string GetTypeName() { return _type; }
        public void Check(string data, string file, string filedName, int line)
        {

        }

        public string Generate(string n)
        {
            var sb = new StringBuilder();
            sb.Append(@"
    private ").Append(_type).Append(" __").Append(n).Append(";");

            sb.Append(@"
    public ").Append(_type).Append(" ").Append(n);
            sb.Append(@"
    {
        internal set{ __").Append(n).Append(" = value; }");
            sb.Append(@"
        get{ return __").Append(n).Append("; }");
            sb.Append(@"
    }");
            return sb.ToString();
        }

        public string GenerateDesc(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append(@"
            ").Append(dst).Append(".").Append(fieldName).Append(" = ").Append(_type).Append(".Parse(").Append(content).Append(");");
            return sb.ToString();
        }
        public string GenerateParse(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append(_type).Append(".Parse(").Append(content).Append(")");
            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperEnum : ITableTypeRuleHelper
    {
        private readonly string _type;
        public TableTypeRuleHelperEnum( string typeDesc )
        {
            var vvc = typeDesc.Split(new string[] { "@" }, System.StringSplitOptions.None);
            _type = vvc[1];
        }
        public string GetTypeName() { return _type; }
        public void Check( string data, string file, string filedName, int line )
        {

        }

        public string Generate( string n )
        {
            var sb = new StringBuilder();
            sb.Append(@"
    private ").Append(_type).Append(" __").Append(n).Append(";");

            sb.Append(@"
    public ").Append(_type).Append(" ").Append(n);
            sb.Append(@"
    {
        internal set{ __").Append(n).Append(" = value; }");
            sb.Append(@"
        get{ return __").Append(n).Append("; }");

            sb.Append(@"
    }");
            return sb.ToString();
        }

        public string GenerateDesc( string fieldName, string content, string dst )
        {

            var sb = new StringBuilder();
            sb.Append(@"
            string str").Append(fieldName).Append(" = ").Append(content).Append(@";
            if (!string.IsNullOrEmpty(str").Append(fieldName).Append(@"))
                ").Append(dst).Append(".").Append(fieldName).Append(" = (").Append(_type).Append(")System.Enum.Parse(typeof(").Append(_type).Append("),str").Append(fieldName).Append(");");
            return sb.ToString();
        }

        public string GenerateParse(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append("(").Append(_type).Append(")System.Enum.Parse(typeof(").Append(_type).Append("),").Append(content).Append(")");
            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperSByte : ITableTypeRuleHelper
    {
        public string GetTypeName() { return "sbyte"; }
        public void Check( string data, string file, string filedName, int line )
        {
            if (data.Length == 0) return;
            try
            {
                var t = sbyte.Parse( data );

            }catch( System.Exception  )
            {
                throw new System.Exception($"[{file}:{line}] {filedName}字段值{data}超出sbyte范围");
            }
        }

        public string Generate( string n )
        {
            var ss = @"
    private sbyte __" + n + ";";
            ss += @"
    public sbyte " + n + @"
    {
        internal set{ __" + n + @" = value; }";

            ss += @"
        get{ return __" + n + @"; }
    }";
 
            return ss;
        }

        public string GenerateDesc( string fieldName, string content, string dst )
        {
            
            var sb = new StringBuilder();
            sb.Append(@"
           ").Append(dst).Append(".").Append( fieldName ).Append(" = sbyte.Parse(").Append( content ).Append(");");
            return sb.ToString();
        }

        public string GenerateParse(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append("sbyte.Parse(").Append(content).Append(")");
            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperByte : ITableTypeRuleHelper
    {
        public string GetTypeName() { return "byte"; }
        public void Check(string data, string file, string filedName, int line)
        {
            if (data.Length == 0) return;
            try
            {
                var t = byte.Parse(data);

            }
            catch (System.Exception )
            {
                throw new System.Exception($"[{file}:{line}] {filedName}字段值{data}超出byte范围");
            }
        }

        public string Generate( string n )
        {
            var ss = @"
    private byte __" + n + ";";
            ss += @"
    public byte " + n + @"
    {
        internal set{ __" + n + @" = value; }";

            ss += @"
        get{ return __" + n + @"; }
    }";

            return ss;
        }


        public string GenerateDesc(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append(@"
           ").Append(dst).Append(".").Append(fieldName).Append(" = byte.Parse(").Append(content).Append(");");
            return sb.ToString();
        }

        public string GenerateParse(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append("byte.Parse(").Append(content).Append(")");
            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperShort : ITableTypeRuleHelper
    {
        public string GetTypeName() { return "short"; }
        public void Check(string data, string file, string filedName, int line)
        {
            if (data.Length == 0) return;
            try
            {
                var t = short.Parse(data);

            }
            catch (System.Exception )
            {
                throw new System.Exception($"[{file}:{line}] {filedName}字段值{data}超出short范围");
            }
        }

        public string Generate( string n )
        {
            var ss = @"
    private short __" + n + ";";
            ss += @"
    public short " + n + @"
    {
        internal set{ __" + n + @" = value; }";

            ss += @"
        get{ return __" + n + @"; }
    }";

            return ss;
        }

        public string GenerateDesc(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append(@"
           ").Append(dst).Append(".").Append(fieldName).Append(" = short.Parse(").Append(content).Append(");");
            return sb.ToString();
        }
        public string GenerateParse(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append("short.Parse(").Append(content).Append(")");
            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperUShort : ITableTypeRuleHelper
    {
        public string GetTypeName() { return "ushort"; }
        public void Check(string data, string file, string filedName, int line)
        {
            if (data.Length == 0) return;
            try
            {
                var t = ushort.Parse(data);

            }
            catch (System.Exception)
            {
                throw new System.Exception($"[{file}:{line}] {filedName}字段值{data}超出ushort范围");
            }
        }

        public string Generate( string n )
        {
            var ss = @"
    private ushort __" + n + ";";
            ss += @"
    public ushort " + n + @"
    {
        internal set{ __" + n + @" = value; }";

            ss += @"
        get{ return __" + n + @"; }
    }";

            return ss;
        }


        public string GenerateDesc(string fieldName, string content, string dst)
        {


            var sb = new StringBuilder();
            sb.Append(@"
           ").Append(dst).Append(".").Append(fieldName).Append(" = ushort.Parse(").Append(content).Append(");");
            return sb.ToString();
        }

        public string GenerateParse(string fieldName, string content, string dst)
        {


            var sb = new StringBuilder();
            sb.Append("ushort.Parse(").Append(content).Append(")");
            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperInt : ITableTypeRuleHelper
    {
        public string GetTypeName() { return "int"; }
        public void Check(string data, string file, string filedName, int line)
        {
            if (data.Length == 0) return;
            try
            {
                var t = int.Parse(data);

            }
            catch (System.Exception)
            {
                throw new System.Exception($"[{file}:{line}] {filedName}字段值{data}超出int范围");
            }
        }

        public string Generate( string n )
        {
            var ss = @"
    private int __" + n + ";";
            ss += @"
    public int " + n + @"
    {
        internal set{ __" + n + @" = value; }";

            ss += @"
        get{ return __" + n + @"; }
    }";

            return ss;
        }


        public string GenerateDesc(string fieldName, string content, string dst)
        {
            var sb = new StringBuilder();
            sb.Append(@"

            string str").Append(fieldName).Append(" = ").Append(content).Append(@";
            if (!string.IsNullOrEmpty(str").Append(fieldName).Append("))").Append(@"
                ").Append(dst).Append(".").Append(fieldName).Append(" = int.Parse(str").Append(fieldName).Append(");");
            return sb.ToString();
        }
        public string GenerateParse(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append("int.Parse(").Append(content).Append(")");
            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperUInt : ITableTypeRuleHelper
    {
        public string GetTypeName() { return "uint"; }
        public void Check(string data, string file, string filedName, int line)
        {
            if (data.Length == 0) return;
            try
            {
                var t = uint.Parse(data);

            }
            catch (System.Exception)
            {
                throw new System.Exception($"[{file}:{line}] {filedName}字段值{data}超出uint范围");
            }
        }

        public string Generate( string n )
        {
            var ss = @"
    private uint __" + n + ";";
            ss += @"
    public uint " + n + @"
    {
        internal set{ __" + n + @" = value; }";

            ss += @"
        get{ return __" + n + @"; }
    }";

            return ss;
        }


        public string GenerateDesc(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append(@"
            ").Append(dst).Append(".").Append(fieldName).Append(" = uint.Parse(").Append(content).Append(");");
            return sb.ToString();
        }
        public string GenerateParse(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append("uint.Parse(").Append(content).Append(")");
            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperLong : ITableTypeRuleHelper
    {
        public string GetTypeName() { return "long"; }
        public void Check(string data, string file, string filedName, int line)
        {
            if (data.Length == 0) return;
            try
            {
                var t = long.Parse(data);

            }
            catch (System.Exception)
            {
                throw new System.Exception($"[{file}:{line}] {filedName}字段值{data}超出long范围");
            }
        }

        public string Generate( string n )
        {
            var ss = @"
    private long __" + n + ";";
            ss += @"
    public long " + n + @"
    {
        internal set{ __" + n + @" = value; }";

            ss += @"
        get{ return __" + n + @"; }
    }";

            return ss;
        }


        public string GenerateDesc(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append(@"
            ").Append(dst).Append(".").Append(fieldName).Append(" = long.Parse(").Append(content).Append(");");
            return sb.ToString();
        }
        public string GenerateParse(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append("long.Parse(").Append(content).Append(")");
            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperFloat : ITableTypeRuleHelper
    {
        public string GetTypeName() { return "float"; }
        public void Check(string data, string file, string filedName, int line)
        {
            if (data.Length == 0) return;
            try
            {
                var t = float.Parse(data);

            }
            catch (System.Exception)
            {
                throw new System.Exception($"[{file}:{line}] {filedName}字段值{data}超出float范围");
            }
        }

        public string Generate(string n)
        {
            var ss = @"
    private float __" + n + ";";
            ss += @"
    public float " + n + @"
    {
        set{ __" + n + @" = value; }";

            ss += @"
        get{ return __" + n + @"; }
    }";

            return ss;
        }


        public string GenerateDesc(string fieldName, string content, string dst)
        {
            var sb = new StringBuilder();
            sb.Append(@"

            string str").Append(fieldName).Append(" = ").Append(content).Append(@";
            if (!string.IsNullOrEmpty(str").Append(fieldName).Append("))").Append(@"
                ").Append(dst).Append(".").Append(fieldName).Append(" = float.Parse(str").Append(fieldName).Append(", System.Globalization.CultureInfo.InvariantCulture);");
            return sb.ToString();
        }
        public string GenerateParse(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append("float.Parse(").Append(content).Append(", System.Globalization.CultureInfo.InvariantCulture)");
            return sb.ToString();
        }
    }
    
    internal class TableTypeRuleHelperBool : ITableTypeRuleHelper
    {
        public string GetTypeName() { return "bool"; }
        public void Check(string data, string file, string filedName, int line)
        {
            if (data.Length == 0) return;
            try
            {
                var t = bool.Parse(data);
            }
            catch (System.Exception)
            {
                throw new System.Exception($"[{file}:{line}] {filedName}字段值{data}不是bool值");
            }
        }

        public string Generate(string n)
        {
            var ss = @"
    private bool __" + n + ";";
            ss += @"
    public bool " + n + @"
    {
        set{ __" + n + @" = value; }";

            ss += @"
        get{ return __" + n + @"; }
    }";

            return ss;
        }


        public string GenerateDesc(string fieldName, string content, string dst)
        {
            var sb = new StringBuilder();
            sb.Append(@"

            string str").Append(fieldName).Append(" = ").Append(content).Append(@";
            if (!string.IsNullOrEmpty(str").Append(fieldName).Append("))").Append(@"
                ").Append(dst).Append(".").Append(fieldName).Append(" = string.Equals(bool.TrueString, str").Append(fieldName).Append(", System.StringComparison.InvariantCultureIgnoreCase);");
            return sb.ToString();
        }
        public string GenerateParse(string fieldName, string content, string dst)
        {
            return $"string.Equals(bool.TrueString, {content}, System.StringComparison.InvariantCultureIgnoreCase)";
        }
    }

    internal class TableTypeRuleHelperULong : ITableTypeRuleHelper
    {
        public string GetTypeName() { return "ulong"; }
        public void Check(string data, string file, string filedName, int line)
        {
            if (data.Length == 0) return;
            try
            {
                var t = ulong.Parse(data);

            }
            catch (System.Exception)
            {
                throw new System.Exception($"[{file}:{line}] {filedName}字段值{data}超出ulong范围");
            }
        }

        public string Generate( string n )
        {
            var ss = @"
    private ulong __" + n + ";";
            ss += @"
    public ulong " + n + @"
    {
        internal set{ __" + n + @" = value; }";

            ss += @"
        get{ return __" + n + @"; }
    }";

            return ss;
        }


        public string GenerateDesc(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append(@"
            ").Append(dst).Append(".").Append(fieldName).Append(" = ulong.Parse(").Append(content).Append(");");
            return sb.ToString();
        }
        public string GenerateParse(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append("ulong.Parse(").Append(content).Append(")");
            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperString : ITableTypeRuleHelper
    {
        public string GetTypeName() { return "string"; }
        public void Check(string data, string file, string filedName, int line)
        {
            //do nothing
        }

        public string Generate( string n )
        {
            var ss = @"
    private string __" + n + ";";
            ss += @"
    public string " + n + @"
    {
        internal set{ __" + n + @" = value; }";

            ss += @"
        get{ return __" + n + @"; }
    }";

            return ss;
        }


        public string GenerateDesc(string fieldName, string content, string dst)
        {

            var sb = new StringBuilder();
            sb.Append(@"
            ").Append(dst).Append(".").Append(fieldName).Append(" = ").Append(content).Append(";");
            return sb.ToString();
        }

        public string GenerateParse(string fieldName, string content, string dst)
        {
            return content;
        }
    }

//     internal class TypeRuleHelperNumber : ITypeRuleHelper
//     {
//         public string GetTypeName() { return "IM.Number"; }
//         public void Check( string data, string file, string filedName, int line )
//         {
//             //do nothing
//         }
//
//         public string Generate( string n )
//         {
//             var ss = @"
//     private IM.Number __" + n + ";";
//             ss += @"
//     public IM.Number " + n + @"
//     {
//         internal set{ __" + n + @" = value; }";
//
//             ss += @"
//         get{ return __" + n + @"; }
//     }";
//
//             return ss;
//         }
//
//
//         public string GenerateDesc(string fieldName, string content, string dst)
//         {
//             var sb = new StringBuilder();
//             sb.Append(@"
//
//             string str").Append(fieldName).Append(" = ").Append(content).Append(@";
//             if (!string.IsNullOrEmpty(str").Append(fieldName).Append("))").Append(@"
//                 ").Append(dst).Append(".").Append(fieldName).Append(" = IM.Number.Parse(str").Append(fieldName).Append(");");
//             return sb.ToString();
//         }
//
//         public string GenerateParse(string fieldName, string content, string dst)
//         {
//
//             var sb = new StringBuilder();
//             sb.Append("IM.Number.Parse(").Append(content).Append(")");
//             return sb.ToString();
//         }
//     }

//     internal class TypeRuleHelperPrecNumber : ITypeRuleHelper
//     {
//         public string GetTypeName() { return "IM.PrecNumber"; }
//         public void Check(string data, string file, string filedName, int line)
//         {
//             //do nothing
//         }
//
//         public string Generate(string n)
//         {
//             var ss = @"
//     private IM.PrecNumber __" + n + ";";
//             ss += @"
//     public IM.PrecNumber " + n + @"
//     {
//         internal set{ __" + n + @" = value; }";
//
//             ss += @"
//         get{ return __" + n + @"; }
//     }";
//
//             return ss;
//         }
//
//
//         public string GenerateDesc(string fieldName, string content, string dst)
//         {
//             var sb = new StringBuilder();
//             sb.Append(@"
//
//             string str").Append(fieldName).Append(" = ").Append(content).Append(@";
//             if (!string.IsNullOrEmpty(str").Append(fieldName).Append("))").Append(@"
//                 ").Append(dst).Append(".").Append(fieldName).Append(" = IM.PrecNumber.Parse(str").Append(fieldName).Append(");");
//             return sb.ToString();
//         }
//
//         public string GenerateParse(string fieldName, string content, string dst)
//         {
//
//             var sb = new StringBuilder();
//             sb.Append("IM.PrecNumber.Parse(").Append(content).Append(")");
//             return sb.ToString();
//         }
//     }

    // internal class TypeRuleHelperDList : ITypeRuleHelper
    // {
    //     public string type_name;
    //     private readonly TypeChecker _checker;
    //
    //     public TypeRuleHelperDList(string name)
    //     {
    //         type_name = name;
    //         var reg = new Regex("^DList");
    //         type_name = reg.Replace( type_name, "List");
    //         var tmp = type_name.Replace(">", "");
    //         var ipos = tmp.IndexOf("<");
    //         tmp = tmp.Substring(ipos + 1);
    //
    //         _checker = new TypeChecker();
    //         _checker.CreateRuler(tmp);
    //     }
    //     public string GetTypeName() { return type_name; }
    //     public void Check(string data, string file, string filedName, int line)
    //     {
    //         if (data.Length == 0) return;
    //         var ll = data.Split(new string[] { "&" }, System.StringSplitOptions.None);
    //         foreach (var n in ll)
    //         {
    //             _checker.Check(n, file, filedName, line);
    //         }
    //     }
    //
    //     public string Generate(string n)
    //     {
    //         return String.Format(@"
    // public List<{0}> {1} = new List<{0}>();", _checker.GetFieldTypeName(), n).ToString();
    //     }
    //
    //     public string GenerateParse( string f, string c, string d ) { return "tmpL"; }
    //     public string GenerateDesc(string fieldName, string content, string dst)
    //     {
    //         var sb = new StringBuilder();
    //
    //         var listType = String.Format(@"
    //                 List<{0}> tmpL = new List<{0}>();", _checker.GetFieldTypeName()).ToString();
    //         
    //         sb.Append( listType );
    //         sb.Append(@"
    //                 string[] list_element = ").Append(content).Append(".Split(new stirng[]{\"&\"}, System.StringSplitOptions.None);");
    //
    //         sb.Append(@"
    //                 for( string vvv in list_element)
    //                 {
    //                     tmpL.Add( ").Append(_checker.GenerateParse(fieldName, "vvv", dst)).Append(");"); ;
    //         sb.Append(@"
    //                 }");
    //
    //         return sb.ToString();
    //     }
    // }

    internal class TableTypeRuleHelperList : ITableTypeRuleHelper
    {
        public string type_name;
        private readonly TableTypeChecker _checker;

        public TableTypeRuleHelperList( string name )
        {
            type_name = name;
            var tmp = type_name.Replace(">", "");
            var ipos = tmp.IndexOf("<");
            tmp = tmp.Substring(ipos + 1);

            _checker = new TableTypeChecker();
            _checker.CreateRuler(tmp);
        }
        public string GetTypeName() { return type_name; }
        public void Check(string data, string file, string filedName, int line)
        {
            if (data.Length == 0) return;
            var ll = data.Split(new string[] { "|" }, System.StringSplitOptions.None);
            foreach (var n in ll)
            {
                _checker.Check(n, file, filedName, line);
            }
        }

        public string Generate( string n )
        {
            return String.Format(@"
    public List<{0}> {1} = new List<{0}>();", _checker.GetFieldTypeName(), n).ToString();
        }

        public string GenerateParse(string f, string c, string d) { return ""; }
        public string GenerateDesc(string fieldName, string content, string dst)
        {
            var sb = new StringBuilder();
            sb.Append(@"
            do{");
            sb.Append(@"
                string[] llc = ").Append( content ).Append( ".Split(new string[]{\"").Append(FlagChar.ListSeparator).Append("\"} ,System.StringSplitOptions.None);");
            sb.Append(@"
                foreach( string vvv in llc )
                {
                    if(vvv.Length == 0 ) 
                        continue;");

            var vParse = _checker.GenerateParse(fieldName, "vvv", dst);
            sb.Append(@"
                    ").Append(dst).Append(".").Append(fieldName).Append(".Add( ").Append(vParse).Append(" );");
                 
            sb.Append(@"
                }");
            sb.Append(@"
            }while(false);");

            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperDDictionary : ITableTypeRuleHelper
    {
        public string type_name;
        private readonly TableTypeChecker _keyChecker;
        private readonly TableTypeChecker _valueChecker;

        public TableTypeRuleHelperDDictionary(string name)
        {
            var vv = new Regex(">$");

            type_name = name.Replace("&", ",");
            var tmp = vv.Replace(type_name, "");
            var ipos = tmp.IndexOf("<");
            tmp = tmp.Substring(ipos + 1);

            var vvc = tmp.Split(new string[] { "," }, System.StringSplitOptions.None);
            _keyChecker = new TableTypeChecker();
            _keyChecker.CreateRuler(vvc[0]);

            _valueChecker = new TableTypeChecker();
            var rL = new Regex("List");
            var value = vvc[1];
            value = rL.Replace( value, "DList");
            _valueChecker.CreateRuler( value );
        }
        public string GetTypeName() { return type_name; }
        public void Check(string data, string file, string filedName, int line)
        {
            if (data.Length == 0) return;

            var ll = data.Split(new string[] { "|" }, System.StringSplitOptions.None);
            foreach (var n in ll)
            {
                var llc = n.Split(new string[] { ":" }, System.StringSplitOptions.None);

                if (llc.Length != 2)
                    throw new System.Exception($"[{file}:{line}] {filedName}字段 {data}填写错误");

                _keyChecker.Check(llc[0], file, filedName, line);
                _valueChecker.Check(llc[1], file, filedName, line);
            }
        }
        public string Generate(string n)
        {

            return String.Format(@"
    public Dictionary<{0},{1}> {2} = new Dictionary<{0},{1}>();", _keyChecker.GetFieldTypeName(), _valueChecker.GetFieldTypeName(), n).ToString();

        }
        public string GenerateParse(string f, string c, string d) { return ""; }
        public string GenerateDesc(string fieldName, string content, string dst)
        {
            var sb = new StringBuilder();

            var key = new StringBuilder();
            var value = new StringBuilder();
            key.Append(_keyChecker.GenerateParse(fieldName, "v2[0]", dst));
            value.Append(_valueChecker.GenerateDesc(fieldName, "v2[1]", dst));
            
            sb.Append(@"
            do{");
            sb.Append(@"
                string[] v1 = ").Append(content).Append(".Split(new string[]{").Append(" \"|\"}").Append(",System.StringSplitOptions.None);");
            sb.Append(@"
                foreach( string layer1 in v1 )
                {
                    if( layer1.Length == 0 )
                        continue;");

            sb.Append(@"
                    string[] v2 = layer1.Split(new string[]{").Append("\":\"}").Append(",System.StringSplitOptions.None);");
            sb.Append(@"
                    if( v2.Length != 2 )
                        continue;");
            sb.Append(value);
            sb.Append(@"
                    ").Append(dst).Append(".").Append(fieldName).Append(".Add( ").Append(key).Append(",").Append( _valueChecker.GenerateParse(fieldName, "v2[1]", dst)).Append(");");


            sb.Append(@"
                }");
            sb.Append(@"
            }while(false);");

            return sb.ToString();
        }
    }

    internal class TableTypeRuleHelperDictionary : ITableTypeRuleHelper
    {
        public string type_name;
        private readonly TableTypeChecker _keyChecker;
        private readonly TableTypeChecker _valueChecker;

        public TableTypeRuleHelperDictionary( string name )
        {
            var vv = new Regex(">$");
       
            type_name = name.Replace("&", ",");
            var tmp = vv.Replace( type_name, "");
            var ipos = tmp.IndexOf("<");
            tmp = tmp.Substring(ipos + 1);

            var vvc = tmp.Split( new string[] { ","}, System.StringSplitOptions.None );
            _keyChecker = new TableTypeChecker();
            _keyChecker.CreateRuler( vvc[0] );

            _valueChecker = new TableTypeChecker();
            _valueChecker.CreateRuler( vvc[1] );
        }
        public string GetTypeName() { return type_name; }
        public void Check( string data, string file, string filedName, int line )
        {
            if (data.Length == 0) return;

            var ll = data.Split(new string[] { "|" }, System.StringSplitOptions.None);
            foreach (var n in ll)
            {
                var llc = n.Split( new string[] { ":"}, System.StringSplitOptions.None );

                if( llc.Length != 2 )
                    throw new System.Exception($"[{file}:{line}] {filedName}字段 {data}填写错误");

                _keyChecker.Check(llc[0], file, filedName, line );
                _valueChecker.Check(llc[1], file, filedName, line );
            }
        }
        public string Generate( string n )
        {

            return String.Format(@"
    public Dictionary<{0},{1}> {2} = new Dictionary<{0},{1}>();", _keyChecker.GetFieldTypeName(), _valueChecker.GetFieldTypeName(), n ).ToString();
          
        }
        public string GenerateParse(string f, string c, string d) { return ""; }
        public string GenerateDesc(string fieldName, string content, string dst)
        {
            var sb = new StringBuilder();

            var key = new StringBuilder();
            var value = new StringBuilder();
            key.Append(_keyChecker.GenerateParse(fieldName, "v2[0]", dst));

            sb.Append(@"
            do{");
            sb.Append(@"
                string[] v1 = ").Append(content).Append(".Split(new string[]{").Append(" \"|\"}").Append(",System.StringSplitOptions.None);");
            sb.Append(@"
                foreach( string layer1 in v1 )
                {
                    if( layer1.Length == 0 )
                        continue;");

            sb.Append(@"
                    string[] v2 = layer1.Split(new string[]{").Append("\":\"}").Append(",System.StringSplitOptions.None);");
            sb.Append(@"
                    if( v2.Length != 2 )
                        continue;");
            sb.Append(@"
                    ").Append(dst).Append(".").Append(fieldName).Append(".Add( ").Append(key).Append(",").Append(_valueChecker.GenerateParse(fieldName, "v2[1]", dst)).Append(");");


            sb.Append(@"
                }");
            sb.Append(@"
            }while(false);");

            return sb.ToString();
        }
    }

    internal class TableTypeChecker : ITableChecker
    {
        private ITableTypeRuleHelper _tableTypeRuleHelper;

        public bool CreateRuler( string typeDesc )
        {
            if (TypeMatchRegex.SbyteRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperSByte();
            }
            else if (TypeMatchRegex.ByteRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperByte();
            }
            else if (TypeMatchRegex.ShortRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperShort();
            }
            else if (TypeMatchRegex.UshortRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperUShort();
            }
            else if (TypeMatchRegex.IntRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperInt();
            }
            else if (TypeMatchRegex.UintRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperUInt();
            }
            else if (TypeMatchRegex.LongRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperLong();
            }
            else if (TypeMatchRegex.UlongRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperULong();
            }
            else if (TypeMatchRegex.FloatRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperFloat();
            }
            else if (TypeMatchRegex.BoolRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperBool();
            }
            else if (TypeMatchRegex.StringRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperString();
            }
            else if (TypeMatchRegex.ListRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperList(typeDesc);
            }
            else if (TypeMatchRegex.DictListRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperDDictionary(typeDesc);
            }
            else if (TypeMatchRegex.DictRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperDictionary(typeDesc);
            }
            else if (TypeMatchRegex.EnumRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperEnum(typeDesc);
            }
            else if (TypeMatchRegex.ClassRegex.IsMatch(typeDesc))
            {
                _tableTypeRuleHelper = new TableTypeRuleHelperClass(typeDesc);
            }
            else
                _tableTypeRuleHelper = new TableTypeRuleHelperString();

            return _tableTypeRuleHelper != null;
        }
        public bool Initialize( string desc )
        {
           var typeDesc = desc.Split( new string[] { ":"}, System.StringSplitOptions.None ).ElementAt(2);
        
           return CreateRuler( typeDesc );
        }
        public string GenerateRowData(string n)
        {
            return _tableTypeRuleHelper.Generate(n);
        }
        public string GenerateDesc(string fileName, string content, string dst)
        {
            return _tableTypeRuleHelper.GenerateDesc(fileName, content, dst);
        }
        public string GenerateParse(string fieldName, string content, string dst)
        {
            return _tableTypeRuleHelper.GenerateParse(fieldName, content, dst);
        }
        public void Check( string data, string file, string fieldName, int line )
        {
            if (_tableTypeRuleHelper != null)
                _tableTypeRuleHelper.Check(data, file, fieldName, line);
            else
                throw new System.Exception($"[{file}:{line}] {fieldName}字段类型错误");
        }

        public string GetFieldTypeName() { return _tableTypeRuleHelper.GetTypeName(); }
    }
    
    internal class TableRuleChecker
    {
        private static readonly Regex RegexUnique = new Regex("^[^:]+:unique$");
        private static readonly Regex RegexNotEmpty = new Regex("^[^:]+:notempty$");
        private static readonly Regex RegexType = new Regex("^[^:]+:type:.*$");
        private static readonly Regex RegexSecondKey = new Regex("^[^:]+:secondarykey$");
        private static readonly Regex RegexThirdKey = new Regex("^[^:]+:thirdkey$");
        
        private string _uniqueFieldName = "";
        private string _secondFieldName = "";
        private string _thirdFieldName = "";
        private readonly Dictionary<string, List<ITableChecker>> _checkerDictionary   = new Dictionary<string, List<ITableChecker>>();
        private readonly Dictionary<string, string> _typeNameList = new Dictionary<string, string>();

        private readonly ILogger _logger;
        
        public TableRuleChecker(ILogger logger)
        {
            _logger = logger;
        }
        
        public string UniqueFiledName
        {
            get { return _uniqueFieldName; }
        }

        public string SecondaryFiledName
        {
            get { return _secondFieldName; }
        }

        public string ThirdFiledName
        {
            get { return _thirdFieldName; }
        }

        public string GetFieldType( string filedName )
        {
            string s;
            if (_typeNameList.TryGetValue(filedName, out s))
                return s;

            return "";
        }
        public List<ITableChecker> GetChecker( string s )
        {
            List<ITableChecker> v;
            if (_checkerDictionary.TryGetValue(s, out v))
                return v;

            return new List<ITableChecker>();
        }
        public void AddChecker( string s, ITableChecker c )
        {
            List<ITableChecker> l = null;
            if(!_checkerDictionary.TryGetValue(s, out l))
            {
                l = new List<ITableChecker>();
                l.Add(c);
                _checkerDictionary.Add(s, l);
            }
            else
            {
                l.Add(c);
            }
        }
        public bool GenerateRule( string r, string file, int lineCount )
        {
            var r1 = r.Replace(']', '|');
            var r2 = r1.Replace('[', '|');

            var array = r2.Split( new string[] { "|"}, System.StringSplitOptions.RemoveEmptyEntries );
            
            foreach( var s in array )
            {
              
                // if (s == "Command:thirdkey")
                // {
                //     var k = 1;
                // }
              
                var fieldsName = s.Split( new string[] { ":"}, System.StringSplitOptions.None ).ElementAt(0);
                fieldsName = TableFileChecker.NormalizeFieldName(fieldsName);
                if (RegexUnique.IsMatch(s))
                {
                    if (_uniqueFieldName.Length == 0)
                        _uniqueFieldName = fieldsName;

                    var c = new TableUniqueChecker();
                    if (c.Initialize(s))
                        AddChecker(fieldsName, c);
                    else
                    {
                        _logger.LogError($"[{file}:{lineCount}] configure error:[{s}]");
                        return false;
                    }

                }
                else if (RegexSecondKey.IsMatch(s))
                {
                    if (_secondFieldName.Length == 0)
                        _secondFieldName = fieldsName;
                }
                else if (RegexThirdKey.IsMatch(s))
                {
                    if (_thirdFieldName.Length == 0)
                        _thirdFieldName = fieldsName;
                }
                else if (RegexNotEmpty.IsMatch(s))
                {
                    var c = new TableNotEmptyChecker();
                    if (c.Initialize(s))
                        AddChecker(fieldsName, c);
                    else
                    {
                        _logger.LogError($"[{file}:{lineCount}] configure error:[{s}]");
                        return false;
                    }
                }
                else if (RegexType.IsMatch(s))
                {
                    var c = new TableTypeChecker();
                    if (c.Initialize(s))
                        AddChecker(fieldsName, c);
                    else
                    {
                        _logger.LogError($"[{file}:{lineCount}] configure error:[{s}]");
                        return false;
                    }

                    _typeNameList.Add(fieldsName, c.GetFieldTypeName());
                }
                else
                {
                    _logger.LogError($"[{file}:{lineCount}] configure error:[{s}]");
                    return false;
                }
            }
            return true;
        }
    }
}
