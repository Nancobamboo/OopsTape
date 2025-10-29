using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public partial class ConfigSystem : YSingletonModule<ConfigSystem>
{
    protected override void OnInit()
    {
        //LoadGameConfig();
    }

    #region Load Interface
    public void LoadJsonConfig<T>(string cfgFile, string idName, ref Dictionary<int, T> cfgs) where T : IConfig, new()
    {
        foreach (JToken jtoken in ((JArray)JsonConvert.DeserializeObject((Resources.Load(cfgFile) as TextAsset).text)))
        {
            JObject jobject = (JObject)jtoken;
            T value = Activator.CreateInstance<T>();
            value.LoadFromJson(jobject);
            var key = (int)jobject[idName];
            if (cfgs.ContainsKey(key))
            {
                Debug.LogError("Conatins Key " + key);
            }
            else
            {
                cfgs.Add(key, value);
            }
        }
    }

    public void LoadJsonConfig<T>(string cfgFile, string idName, ref Dictionary<string, T> cfgs) where T : IConfig, new()
    {
        foreach (JToken jtoken in ((JArray)JsonConvert.DeserializeObject((Resources.Load(cfgFile) as TextAsset).text)))
        {
            JObject jobject = (JObject)jtoken;
            T value = Activator.CreateInstance<T>();
            value.LoadFromJson(jobject);
            var key = (string)jobject[idName];
            if (cfgs.ContainsKey(key))
            {
                Debug.LogError("Conatins Key " + key);
            }
            else
            {
                cfgs.Add(key, value);
            }
        }
    }



    public void LoadJsonConfig<T>(string cfgFile, ref List<T> cfgs) where T : IConfig, new()
    {
        foreach (JToken jtoken in ((JArray)JsonConvert.DeserializeObject((Resources.Load(cfgFile) as TextAsset).text)))
        {
            JObject jobject = (JObject)jtoken;
            T item = Activator.CreateInstance<T>();
            item.LoadFromJson(jobject);
            cfgs.Add(item);
        }
    }

    #endregion
}
