
using System;
using System.Collections.Generic;
using System.Reflection;

public static partial class ConfigManager 
{
    public static Dictionary<TK, TV> LoadTableJson<TK, TV>(string file)
    {
        var res = UnityEngine.Resources.Load<UnityEngine.TextAsset>(file);
        if (res) return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<TK, TV>>(res.text);
        UnityEngine.Debug.LogWarning($"load text error file:{file}");
        return default;
    }
    public static T LoadTableJson<T>(string file)
    {
        var res = UnityEngine.Resources.Load<UnityEngine.TextAsset>(file);
        if (res) return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(res.text);
        UnityEngine.Debug.LogWarning($"load text error file:{file}");
        return default;
    }

    public static void ParseConstJson(Type mytype, string file)
    {
        var cfg = LoadTableJson<Dictionary<string, object>>(file);
        foreach (var info in mytype.GetFields()) {
            if (!cfg.ContainsKey(info.Name)) continue;
            ReflectionSetValue(mytype, info, cfg[info.Name]);
        }
    }
    
    private static void ReflectionSetValue<T>(T cls, FieldInfo field, object value)
    {
        if (value is Newtonsoft.Json.Linq.JArray || value is Newtonsoft.Json.Linq.JObject)
            field.SetValue(cls, Newtonsoft.Json.JsonConvert.DeserializeObject(value.ToString(), field.FieldType));
        else
            switch (field.FieldType.Name)
            {
                case "Int32":
                    field.SetValue(cls, Convert.ToInt32(value));
                    break;
                case "Single":
                    field.SetValue(cls, Convert.ToSingle(value));
                    break;
                default:
                    field.SetValue(cls, value);
                    break;
            }
    }
}
