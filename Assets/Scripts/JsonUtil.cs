using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class JsonUtil
{
    public static JArray LoadFile(string jsonfile)
    {
        FileInfo fileInfo = new FileInfo(jsonfile);
        if (fileInfo.Exists)
        {
            using (StreamReader reader = new StreamReader(jsonfile))
            {
                string str = reader.ReadToEnd();
                JArray jarray = (JArray)JsonConvert.DeserializeObject(str);
                reader.Close();
                return jarray;
            }
        }
        else
        {
            Debug.LogWarning("no exist json file: " + jsonfile);
        }
        return null;
    }

    // ------------------------------------------------------------------
    // object -> json
    public static JArray ToJArray<T>(List<T> inlist)
    {
        bool isTypeIData = typeof(IData).IsAssignableFrom(typeof(T));
        JArray jarray = new JArray();
        foreach (T obj in inlist)
        {
            if (isTypeIData)
            {
                JObject jobj = new JObject();
                ((IData)obj).SaveToJson(jobj);
                jarray.Add(jobj);
            }
            else
                jarray.Add(obj);
        }
        return jarray;
    }

    public static JObject ToJObject<T1, T2>(Dictionary<T1, T2> dictionary)
    {
        JObject jobj = new JObject();
        foreach (KeyValuePair<T1, T2> kvp in dictionary)
        {
            jobj.Add(kvp.Key.ToString(), kvp.Value.ToString());
        }
        return jobj;
    }

    // ------------------------------------------------------------------
    // json -> object
    public static bool ToList<T>(JObject obj, string key, ref List<T> outlist) where T : new()
    {
        if (!obj.ContainsKey(key))
        {
            return false;
        }
        if (outlist == null)
        {
            outlist = new List<T>();
        }

        bool isTypeIData = typeof(IData).IsAssignableFrom(typeof(T));

        bool isTypeIConfig = typeof(IConfig).IsAssignableFrom(typeof(T));

        JArray jarray = (JArray)obj[key];
        foreach (object jobj in jarray)
        {
            if (isTypeIData)
            {
                T nt = new T();
                ((IData)nt).LoadFromJson((JObject)jobj);
                outlist.Add(nt);
            }
            else if (isTypeIConfig)
            {
                T nt = new T();
                ((IConfig)nt).LoadFromJson((JObject)jobj);
                outlist.Add(nt);
            }
            else
                outlist.Add(ToValue<T>(jobj));
        }
        return true;
    }
    public static bool ToDict<T1, T2>(JObject obj, string key, ref Dictionary<T1, T2> outdict)
    {
        if (!obj.ContainsKey(key))
            return false;

        JObject jobj = (JObject)obj[key];
        foreach (var kv in jobj)
        {
            outdict[ToValue<T1>(kv.Key)] = ToValue<T2>(kv.Value);
        }
        return true;
    }

    public static bool ToList(JObject obj, string key, ref List<string> outlist)
    {
        if (!obj.ContainsKey(key))
        {
            return false;
        }
        if (outlist == null)
        {
            outlist = new List<string>();
        }

        JArray jarray = (JArray)obj[key];
        foreach (var jobj in jarray.Values())
        {

            outlist.Add((string)(jobj));
        }
        return true;
    }


    public static T ToValue<T>(JObject obj, string key, T defaultVal)
    {
        if (!obj.ContainsKey(key))
            return defaultVal;
        return (T)System.Convert.ChangeType(obj[key], typeof(T));
    }

    public static T ToValue<T>(object val)
    {
        Debug.Assert(val != null, "CHECK");
        return (T)System.Convert.ChangeType(val, typeof(T));
    }

    //public static bool TryGetValue<T>(JObject jobj, string key, ref T value)
    //{
    //    object tmp = new object();
    //    if (jobj.TryGetValue(key, out tmp))
    //    {
    //        value = (T)System.Convert.ChangeType(tmp, typeof(T));
    //        return true;
    //    }
    //    return false;
    //}
}
