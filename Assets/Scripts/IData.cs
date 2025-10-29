using System;
using Newtonsoft.Json.Linq;

public interface IData
{
	void LoadFromJson(JObject jobject);
	void SaveToJson(JObject jobject);
}

public interface IConfig
{
    void LoadFromJson(JObject jobject);
}