using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;

public class YProcessConfigEditor : EditorWindow
{
    public static YProcessConfigEditor Instance;

    public string ExportName;
    public string ConfigNameStr;
    public string[] ConfigNameArray;
    public string[] TypeArray;
    public string JsonName;

    public float HeightLeft;
    const string GICodePath = "Assets/Scripts/GIStruct";

    const string CodePath = "Assets/Scripts/Struct";
    const string ModelPath = "Assets/Scripts/Struct";
    const string ConfigSystemPath = "/Scripts/Model/ConfigSystemMap.cs";
    const string DataSystemPath = "/Scripts/Model/DataSystemMap.cs";


    public string[] VarTypeArray =
    {
        "string",
        "int",
        "float",
        "enum",
        "List<int>",
        "List<float>",
        "List<string>",
        "Dictionary<int, int>",
        "Dictionary<int, string>",
        "bool",
    };


    [MenuItem("EditorTools/Process Config")]
    public static void Open()
    {
        if (Instance != null)
        {
            Instance.Show();
            return;
        }
        Instance = EditorWindow.GetWindow<YProcessConfigEditor>();
        Instance.titleContent = new GUIContent("ProcessUI Editor");

        Instance.Show();
    }


    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        HeightLeft = 20f;

        EditorGUI.LabelField(GetGUIRect(20, 100, 18, false), "ExportName:");
        ExportName = EditorGUI.TextField(GetGUIRect(100, 180, 18, false), ExportName);

        HeightLeft += 20f;
        EditorGUI.LabelField(GetGUIRect(20, 100, 18, false), "ConfingNames:");
        ConfigNameStr = EditorGUI.TextField(GetGUIRect(115, 300, 18, false), ConfigNameStr);

        if (!string.IsNullOrEmpty(ConfigNameStr))
        {
            ConfigNameStr = ConfigNameStr.Replace(" ", "");
            ConfigNameArray = ConfigNameStr.Split('\t');
        }

        HeightLeft += 20;

        if (GUI.Button(GetGUIRect(20, 100, 18, false), "Load Exist Code"))
        {

            LoadJsonName();

            return;
        }

        JsonName = EditorGUI.TextField(GetGUIRect(125, 180, 18, false), JsonName);

        HeightLeft += 20;

        if (ConfigNameArray != null && ConfigNameArray.Length > 0)
        {
            if (this.TypeArray == null)
            {
                this.TypeArray = new string[ConfigNameArray.Length];
                for (int i = 0; i < TypeArray.Length; i++)
                {
                    TypeArray[i] = "int";
                }
            }

            EditorGUI.LabelField(GetGUIRect(40, 100, 18, false), "VarName  ?y");

            HeightLeft += 20;

            for (int j = 0; j < ConfigNameArray.Length; j++)
            {
                GUI.SetNextControlName("ConfingName" + j);
                var rect = GetGUIRect(40, 180, 18, false);
                ConfigNameArray[j] = EditorGUI.TextField(rect, (string)ConfigNameArray[j]);
                GetGUIRect(175, 20, 18);
                HeightLeft += 2;
            }



            HeightLeft += 10;

            if (GUI.Button(GetGUIRect(40, 150, 18), "Reset"))
            {
                ExportName = "TMP";
                TypeArray = null;
                ConfigNameStr = "";
                ConfigNameArray = null;
                return;
            }


            HeightLeft += 10;

            if (GUI.Button(GetGUIRect(40, 150, 18), "Export Config"))
            {
                GenConfigCode();
                AssetDatabase.Refresh();
                return;
            }

            HeightLeft += 10;

            if (GUI.Button(GetGUIRect(40, 150, 18), "Export Data"))
            {
                GenDataCode();
                AssetDatabase.Refresh();
                return;
            }

            HeightLeft += 10;

            if (GUI.Button(GetGUIRect(40, 150, 18), "Export Model"))
            {
                GenModelCode();
                AssetDatabase.Refresh();
                return;
            }

            HeightLeft += 10;

            if (GUI.Button(GetGUIRect(40, 150, 18), "Export GI Calss"))
            {
                GenGIClassCode();
                AssetDatabase.Refresh();
                return;
            }


            HeightLeft = 80;
            EditorGUI.LabelField(GetGUIRect(250, 100, 18, false), "VarType  ?y");

            HeightLeft += 20;

            for (int j = 0; j < TypeArray.Length; j++)
            {
                GUI.SetNextControlName("TypeArray" + j);
                var rect = GetGUIRect(250, 180, 18, false);
                TypeArray[j] = EditorGUI.TextField(rect, (string)TypeArray[j]);
                GetGUIRect(175, 20, 18);
                HeightLeft += 2;
            }

            EditorGUI.LabelField(GetGUIRect(250, 200, 18), "Quick Fill Var Type ?y");

            foreach (var comp in VarTypeArray)
            {
                HeightLeft += 10;
                var compType = comp;

                if (GUI.Button(GetGUIRect(260, 150, 18), compType))
                {
                    string name = GUI.GetNameOfFocusedControl();
                    if (name.Contains("TypeArray"))
                    {
                        var index = name.Substring(9);
                        TypeArray[int.Parse(index)] = compType;
                    }
                    GUIUtility.keyboardControl = 0;
                }
            }

            HeightLeft = 50;
            EditorGUI.LabelField(GetGUIRect(460, 200, 18, false), "Batch Quick Start ?y");

            HeightLeft += 10;

            for (int i = 0; i < VarTypeArray.Length; i++)
            {
                HeightLeft += 10;
                if (GUI.Button(GetGUIRect(460, 150, 18), VarTypeArray[i]))
                {
                    for (int j = 0; j < TypeArray.Length; j++)
                    {
                        TypeArray[j] = VarTypeArray[i];
                    }
                }
            }

        }
    }

    private Rect GetGUIRect(float leftPos, float width, float height, bool addHeight = true)
    {
        Rect rect = new Rect(leftPos, HeightLeft, width, height);
        if (addHeight)
        {
            HeightLeft += height;
        }
        return rect;
    }


    void LoadJsonName()
    {
        var existName = Application.dataPath + "/Scripts/Struct/" + JsonName + ".cs";
        if (JsonName[0] == 'G' && JsonName[1] == 'I')
        {
            existName = Application.dataPath + "/Scripts/GIStruct/" + JsonName + ".cs";
        }

        var lines = File.ReadAllLines(existName);

        if (ConfigNameArray == null || ConfigNameArray.Length == 0)
        {
            List<string> names = new List<string>();
            for (int i = 4; i < lines.Length; i++)
            {
                if (lines[i].Contains("LoadFromJson"))
                {
                    Debug.Log("Load " + JsonName + " Code Succeed");
                    break;
                }

                if (lines[i].Contains("public") && !lines[i].Contains("class"))
                {
                    var equalIndex = lines[i].LastIndexOf(" = ");
                    string tmpLine = lines[i];
                    if (equalIndex > 0)
                    {
                        tmpLine = lines[i].Substring(0, equalIndex);
                        tmpLine += ";";
                        Debug.Log(lines[i]);
                    }

                    var spaceIndex = tmpLine.LastIndexOf(' ') + 1;
                    names.Add(tmpLine.Substring(spaceIndex, tmpLine.Length - 1 - spaceIndex));
                }
            }
            ExportName = JsonName;
            names.Add("NewItem");
            ConfigNameArray = new string[names.Count];
            TypeArray = new string[names.Count];
            for (int i = 0; i < names.Count; i++)
            {
                ConfigNameArray[i] = names[i];
                //ConfigNameStr = ConfigNameStr + names[i] + "\t";
            }
        }

        for (int i = 4; i < lines.Length; i++)
        {
            if (lines[i].Contains("LoadFromJson"))
            {
                Debug.Log("Load " + JsonName + " Code Succeed");
                break;
            }

            for (int j = 0; j < ConfigNameArray.Length; j++)
            {
                if (lines[i].Contains("public") && lines[i].Contains(ConfigNameArray[j]))
                {
                    if (lines[i].Contains("E" + ConfigNameArray[j]))
                    {
                        TypeArray[j] = "enum";
                    }
                    else
                    {
                        int startIndex = lines[i].IndexOf("public") + 7;
                        int endIndex = lines[i].IndexOf(ConfigNameArray[j]) - 1;

                        int totalNum = endIndex - startIndex;

                        TypeArray[j] = lines[i].Substring(startIndex, totalNum);
                    }
                }
            }
        }
    }

    public void GenGIClassCode()
    {
        if (ExportName.Contains("GI"))
        {
            ExportName = ExportName.Remove(0, 2);
        }

        string realDiskCSFilePath = GICodePath + "/GI" + ExportName + ".cs";
        string codeStr = "//This File Is Auto Generated By Process Config\n";
        codeStr += "using Newtonsoft.Json.Linq;\nusing System.Collections.Generic;\n\npublic class GI" + ExportName + " : IConfig\n{\n";

        for (int i = 0; i < ConfigNameArray.Length; i++)
        {
            if (TypeArray[i] == "enum")
            {
                codeStr += "\tpublic E" + ConfigNameArray[i] + " " + ConfigNameArray[i] + ";\n";
            }
            else
            {
                codeStr += "\tpublic " + TypeArray[i] + " " + ConfigNameArray[i] + ";\n";
            }
        }
        codeStr += "\tpublic void LoadFromJson(JObject jobject)\n\t{\n";

        for (int i = 0; i < ConfigNameArray.Length; i++)
        {
            if (TypeArray[i].Contains("List"))
            {
                if (TypeArray[i].Contains("List"))
                {
                    codeStr += "\t\tJsonUtil.ToList(jobject, \"" + ConfigNameArray[i] + "\", ref " + ConfigNameArray[i] + ");\n";
                }
            }
            else
            {
                if (TypeArray[i] == "enum")
                {
                    codeStr += "\t\t" + ConfigNameArray[i] + " = (E" + ConfigNameArray[i]
                        + ")(int)jobject[\"" + ConfigNameArray[i] + "\"];\n";
                }
                else
                {
                    codeStr += "\t\t" + ConfigNameArray[i] + " = (" + TypeArray[i] + ")jobject[\"" + ConfigNameArray[i] + "\"];\n";
                }

            }
        }
        codeStr += "\t}\n}\n\n";

        try
        {
            StreamWriter sw;
            sw = new StreamWriter(new FileStream(realDiskCSFilePath, FileMode.Create, FileAccess.ReadWrite));

            sw.Write(codeStr);
            sw.Close();
            sw.Dispose();
            Debug.Log(realDiskCSFilePath + " Save Succeed");

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
    

    public void GenConfigCode()
    {
        if(ExportName.Contains("Config"))
        {
            ExportName = ExportName.Remove(0,6);
        }


        string realDiskCSFilePath = CodePath + "/Config" + ExportName + ".cs";
        string codeStr = "//This File Is Auto Generated By Process Config\n";
        codeStr += "using Newtonsoft.Json.Linq;\nusing System.Collections.Generic;\n\npublic class Config" + ExportName + " : IConfig\n{\n";

        for (int i = 0; i < ConfigNameArray.Length; i++)
        {
            if (TypeArray[i] == "enum")
            {
                codeStr += "\tpublic E" + ConfigNameArray[i] + " " + ConfigNameArray[i] + ";\n";
            }
            else
            {
                codeStr += "\tpublic " + TypeArray[i] + " " + ConfigNameArray[i] + ";\n";
            }
        }
        codeStr += "\tpublic void LoadFromJson(JObject jobject)\n\t{\n";

        for (int i = 0; i < ConfigNameArray.Length; i++)
        {
            if (TypeArray[i].Contains("List"))
            {
                switch (TypeArray[i])
                {
                    case "List<int>":
                        codeStr += "\t\t" + ConfigNameArray[i] + " = ControlUtil.GetIntValueListByString((string)jobject[\"" + ConfigNameArray[i] + "\"]);\n";
                        break;
                    case "List<string>":
                        codeStr += "\t\t" + ConfigNameArray[i] + " = ControlUtil.GetStringValueListByString((string)jobject[\"" + ConfigNameArray[i] + "\"]);\n";
                        break;
                    case "List<float>":
                        codeStr += "\t\t" + ConfigNameArray[i] + " = ControlUtil.GetFloatValueListByString((string)jobject[\"" + ConfigNameArray[i] + "\"]);\n";
                        break;
                    default:
                        codeStr += "\t\tJsonUtil.ToList(jobject, \"" + ConfigNameArray[i] + "\", ref " + ConfigNameArray[i] + ");\n";
                        break;
                }
            }
            else
            {
                if (TypeArray[i] == "enum")
                {
                    codeStr += "\t\t" + ConfigNameArray[i] + " = (E" + ConfigNameArray[i] 
                        + ")(int)jobject[\"" + ConfigNameArray[i] + "\"];\n";
                }
                else
                {
                    codeStr += "\t\t" + ConfigNameArray[i] + " = (" + TypeArray[i] + ")jobject[\"" + ConfigNameArray[i] + "\"];\n";
                }

            }
        }
        codeStr += "\t}\n}\n\n";

        var className = "Config" + ExportName;
        codeStr += "public partial class ConfigSystem \n{\n\tDictionary<int, " + className + "> m_" + ExportName + "Dict = new Dictionary<int, " + className + ">();\n\n\t";
        codeStr += "public " + className + " Get" + className + "ById(int id)\n\t{\n\t\t";
        codeStr += className + " result = null;\n\t\tm_" + ExportName +"Dict.TryGetValue(id, out result);\n\t\t";
        codeStr += "return result;\n\t}\n\n";

        codeStr += "\tpublic void Load" + className + "()\n\t{\n\t\tLoadJsonConfig(\"Configs/Config_" + ExportName + "\", \"" + ConfigNameArray[0] + "\", ref m_" + ExportName + "Dict);";
        codeStr += "\n\t}\n}";

        try
        {
                        
            StreamWriter sw;
            sw = new StreamWriter(new FileStream(realDiskCSFilePath, FileMode.Create, FileAccess.ReadWrite));

            sw.Write(codeStr);
            sw.Close();
            sw.Dispose();
            Debug.Log(realDiskCSFilePath + " Save Succeed");

            if (className.Contains("TMP"))
            {
                return;
            }

            var exPath = Application.dataPath + ConfigSystemPath;
            var lines = File.ReadAllLines(exPath);
            List<string> list = new List<string>();

            bool isAdded = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(className))
                {
                    isAdded = true;
                    break;
                }
                if (lines[i].Contains("Get"))
                {
                    break;
                }
            }
            if(!isAdded)
            {
                
                for (int i = 0; i < lines.Length; i++)
                {
                    list.Add(lines[i]);
                    if (!isAdded && lines[i].Contains("LoadConfig") && lines[i+1].Contains("}"))
                    {
                        isAdded = true;
                        list.Add("\t\tLoad" + className + "();");
                        Debug.Log("Add Load InterFace Succed");
                    }
                }
                File.WriteAllLines(exPath, list.ToArray());
            }
                    }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }


    public void GenDataCode()
    {
        if (ExportName.Contains("Data"))
        {
            ExportName = ExportName.Remove(0, 4);
        }


        string realDiskCSFilePath = CodePath + "/Data" + ExportName + ".cs";
        string codeStr = "//This File Is Auto Generated By Process Config\n";

        bool useList = false;

        foreach (var item in TypeArray)
        {
            if(item.Contains("List") || item.Contains("Dict"))
            {
                useList = true;
            }
        }

        if(useList)
        {
            codeStr += "using System.Collections.Generic;\nusing System.Linq;\n";
        }

        codeStr += "using Newtonsoft.Json.Linq;\n\npublic class Data" + ExportName + " : IData\n{\n";

        for (int i = 0; i < ConfigNameArray.Length; i++)
        {
            if (TypeArray[i] == "enum")
            {
                codeStr += "\tpublic E" + ConfigNameArray[i] + " " + ConfigNameArray[i] + ";\n";
            }
            else if(TypeArray[i].Contains("List"))
            {
                codeStr += "\tpublic " + TypeArray[i] + " " + ConfigNameArray[i] + " = new "+ TypeArray[i] + "();\n";
            }
            else if(TypeArray[i].Contains("Dict"))
            {
                codeStr += "\tpublic " + TypeArray[i] + " " + ConfigNameArray[i] + " = new " + TypeArray[i] + "();\n";
            }
            else
            {
                codeStr += "\tpublic " + TypeArray[i] + " " + ConfigNameArray[i] + ";\n";
            }
        }
        codeStr += "\tpublic void LoadFromJson(JObject jobject)\n\t{\n";

        List<int> funcIndexs = new List<int>();

        for (int i = 0; i < ConfigNameArray.Length; i++)
        {
            if (TypeArray[i].Contains("List"))
            {
                codeStr += "\t\tJsonUtil.ToList(jobject, \"" + ConfigNameArray[i] + "\", ref " + ConfigNameArray[i] + ");\n";
                funcIndexs.Add(i);
            }
            else if (TypeArray[i].Contains("Dict"))
            {
                //Must Name Is xxDict
                if (TypeArray[i] == "Dictionary<int, int>")
                {
                    string preifix = ConfigNameArray[i].Substring(0, ConfigNameArray[i].Length - 4);
                    string prefixKey = preifix + "Key";
                    string prefixValue = preifix + "Value";
                    codeStr += "\t\tvar " + prefixKey + " = new List<int>();\n";
                    codeStr += "\t\tJsonUtil.ToList(jobject, \"" + prefixKey + "\", ref " + prefixKey + ");\n";
                    codeStr += "\t\tvar " + prefixValue + " = new List<int>();\n";
                    codeStr += "\t\tJsonUtil.ToList(jobject, \"" + prefixValue + "\", ref " + prefixValue + ");\n";

                    codeStr += "\t\tfor(int i = 0; i < " + prefixKey + ".Count; i++" + ")\n\t\t{\n\t\t\t" + ConfigNameArray[i] + "[" + prefixKey + "[i]] = " + prefixValue + "[i];\n\t\t}\n";
                    funcIndexs.Add(i);
                } 
                else if(TypeArray[i] == "Dictionary<int, string>")
                {
                    string preifix = ConfigNameArray[i].Substring(0, ConfigNameArray[i].Length - 4);
                    string prefixKey = preifix + "Key";
                    string prefixValue = preifix + "Value";
                    codeStr += "\t\tvar " + prefixKey + " = new List<int>();\n";
                    codeStr += "\t\tJsonUtil.ToList(jobject, \"" + prefixKey + "\", ref " + prefixKey + ");\n";
                    codeStr += "\t\tvar " + prefixValue + " = new List<string>();\n";
                    codeStr += "\t\tJsonUtil.ToList(jobject, \"" + prefixValue + "\", ref " + prefixValue + ");\n";

                    codeStr += "\t\tfor(int i = 0; i < " + prefixKey + ".Count; i++" + ")\n\t\t{\n\t\t\t" + ConfigNameArray[i] + "[" + prefixKey + "[i]] = " + prefixValue + "[i];\n\t\t}\n";
                    funcIndexs.Add(i);
                }
                else
                {
                    string prefix = ConfigNameArray[i].Substring(0, ConfigNameArray[i].Length - 4) + "List";
                    int number = TypeArray[i].Length - 17;
                    var typeName = TypeArray[i].Substring(16, number);

                    codeStr += "\t\tvar " + prefix + " = new List<" + typeName + ">();\n";
                    codeStr += "\t\tJsonUtil.ToList(jobject, \"" + prefix + "\", ref " + prefix + ");\n";

                    codeStr += "\t\tforeach(var item in " + prefix + ")\n\t\t{\n\t\t\t" + ConfigNameArray[i] + "[item.Id] = item;\n\t\t}\n";
                    funcIndexs.Add(i);
                }


            }
            else
            {
                if (TypeArray[i] == "enum")
                {
                    codeStr += "\t\t" + ConfigNameArray[i] + " = (E" + ConfigNameArray[i]
                        + ")(int)jobject[\"" + ConfigNameArray[i] + "\"];\n";
                }
                else
                {
                    codeStr += "\t\t" + ConfigNameArray[i] + " = (" + TypeArray[i] + ")jobject[\"" + ConfigNameArray[i] + "\"];\n";
                }
            }
        }
        codeStr += "\t}\n";

        codeStr += "\tpublic void SaveToJson(JObject jobject)\n\t{\n";

        for (int i = 0; i < ConfigNameArray.Length; i++)
        {
            if (TypeArray[i].Contains("List"))
            {
                codeStr += "\t\t" + "jobject.Add(\"" + ConfigNameArray[i] + "\", JsonUtil.ToJArray(" + ConfigNameArray[i] + "));\n";
            }
            else if (TypeArray[i].Contains("Dict"))
            {
                //Must Name Is xxDict

                if (TypeArray[i] == "Dictionary<int, int>")
                {
                    string preifix = ConfigNameArray[i].Substring(0, ConfigNameArray[i].Length - 4);
                    string prefixKey = preifix + "Key";
                    string prefixValue = preifix + "Value";

                    codeStr += "\t\tvar " + prefixKey + " = " + ConfigNameArray[i] + ".Keys.ToList();\n";
                    codeStr += "\t\t" + "jobject.Add(\"" + prefixKey + "\", JsonUtil.ToJArray(" + prefixKey + "));\n";
                    codeStr += "\t\tvar " + prefixValue + " = " + ConfigNameArray[i] + ".Values.ToList();\n";
                    codeStr += "\t\t" + "jobject.Add(\"" + prefixValue + "\", JsonUtil.ToJArray(" + prefixValue + "));\n";
                }
                else if (TypeArray[i] == "Dictionary<int, string>")
                {
                    string preifix = ConfigNameArray[i].Substring(0, ConfigNameArray[i].Length - 4);
                    string prefixKey = preifix + "Key";
                    string prefixValue = preifix + "Value";

                    codeStr += "\t\tvar " + prefixKey + " = " + ConfigNameArray[i] + ".Keys.ToList();\n";
                    codeStr += "\t\t" + "jobject.Add(\"" + prefixKey + "\", JsonUtil.ToJArray(" + prefixKey + "));\n";
                    codeStr += "\t\tvar " + prefixValue + " = " + ConfigNameArray[i] + ".Values.ToList();\n";
                    codeStr += "\t\t" + "jobject.Add(\"" + prefixValue + "\", JsonUtil.ToJArray(" + prefixValue + "));\n";
                }
                else
                {
                    string prefix = ConfigNameArray[i].Substring(0, ConfigNameArray[i].Length - 4) + "List";

                    codeStr += "\t\tvar " + prefix + " = " + ConfigNameArray[i] + ".Values.ToList();\n";
                    codeStr += "\t\t" + "jobject.Add(\"" + prefix + "\", JsonUtil.ToJArray(" + prefix + "));\n";
                }
            }
            else
            {
                if (TypeArray[i] == "enum")
                {
                    codeStr += "\t\t" + "jobject.Add(\"" + ConfigNameArray[i] + "\", (int)" + ConfigNameArray[i] + ");\n";
                }
                else
                {
                    codeStr += "\t\t" + "jobject.Add(\"" + ConfigNameArray[i] + "\", " + ConfigNameArray[i] + ");\n";
                }
            }
        }
        codeStr += "\t}\n";

        if (funcIndexs.Count > 0)
        {
            for (int i = 0; i < funcIndexs.Count; i++)
            {
                if (TypeArray[funcIndexs[i]].Contains("List"))
                {
                    int number = TypeArray[funcIndexs[i]].Length - 6;

                    var typeName = TypeArray[funcIndexs[i]].Substring(5, number);
                    codeStr += "\tpublic void Add" + ConfigNameArray[funcIndexs[i]] + "Data(" + typeName + " data)\n";
                    codeStr += "\t{\n\t\t" + ConfigNameArray[funcIndexs[i]] + ".Add(data);\n\t}\n";

                    codeStr += "\tpublic void Remove" + ConfigNameArray[funcIndexs[i]] + "Data(" + typeName + " data)\n";
                    codeStr += "\t{\n\t\t" + ConfigNameArray[funcIndexs[i]] + ".Remove(data);\n\t}\n";

                    codeStr += "\tpublic "+ typeName+ " Get" + ConfigNameArray[funcIndexs[i]] + "Data(int dataIndex)\n";
                    codeStr += "\t{\n\t\treturn " + ConfigNameArray[funcIndexs[i]] + "[dataIndex];\n\t}\n";

                }
                else if(TypeArray[funcIndexs[i]].Contains("Dict"))
                {

                    if (TypeArray[funcIndexs[i]] == "Dictionary<int, int>")
                    {
                        int number = TypeArray[funcIndexs[i]].Length - 17;

                        var typeName = TypeArray[funcIndexs[i]].Substring(16, number);
                        typeName = typeName.Replace(" ", "");
                        codeStr += "\tpublic void Add" + ConfigNameArray[funcIndexs[i]] + "Data(int dataKey, int value)\n";

                        codeStr += "\t{\n\t\t" + ConfigNameArray[funcIndexs[i]] + "[dataKey] = Get" + ConfigNameArray[funcIndexs[i]] + "Data(dataKey) + " + "value;\n\t}\n";

                        codeStr += "\tpublic void Remove" + ConfigNameArray[funcIndexs[i]] + "Data(int dataKey)\n";
                        codeStr += "\t{\n\t\t" + ConfigNameArray[funcIndexs[i]] + ".Remove(dataKey);\n\t}\n";

                        codeStr += "\tpublic " + typeName + " Get" + ConfigNameArray[funcIndexs[i]] + "Data(int dataKey)\n";
                        codeStr += "\t{\n\t\t" + typeName + " result = 0;\n";
                        codeStr += "\t\t" + ConfigNameArray[funcIndexs[i]] + ".TryGetValue(dataKey, out result);\n";
                        codeStr += "\t\treturn result;\n\t}\n";
                    }
                    else if (TypeArray[funcIndexs[i]] == "Dictionary<int, string>")
                    {
                        int number = TypeArray[funcIndexs[i]].Length - 17;

                        var typeName = TypeArray[funcIndexs[i]].Substring(16, number);
                        typeName = typeName.Replace(" ", "");
                        codeStr += "\tpublic void Add" + ConfigNameArray[funcIndexs[i]] + "Data(int dataKey, string value)\n";

                        codeStr += "\t{\n\t\t" + ConfigNameArray[funcIndexs[i]] + "[dataKey] = value;\n\t}\n";

                        codeStr += "\tpublic void Remove" + ConfigNameArray[funcIndexs[i]] + "Data(int dataKey)\n";
                        codeStr += "\t{\n\t\t" + ConfigNameArray[funcIndexs[i]] + ".Remove(dataKey);\n\t}\n";

                        codeStr += "\tpublic " + typeName + " Get" + ConfigNameArray[funcIndexs[i]] + "Data(int dataKey)\n";
                        codeStr += "\t{\n\t\t" + typeName + " result = \"\";\n";
                        codeStr += "\t\t" + ConfigNameArray[funcIndexs[i]] + ".TryGetValue(dataKey, out result);\n";
                        codeStr += "\t\treturn result;\n\t}\n";

                    }
                    else
                    {
                        int number = TypeArray[funcIndexs[i]].Length - 17;

                        var typeName = TypeArray[funcIndexs[i]].Substring(16, number);
                        typeName = typeName.Replace(" ", "");
                        codeStr += "\tpublic void Add" + ConfigNameArray[funcIndexs[i]] + "Data(" + typeName + " item)\n";
                        codeStr += "\t{\n\t\t" + ConfigNameArray[funcIndexs[i]] + "[item.Id] = item;\n\t}\n";

                        codeStr += "\tpublic void Remove" + ConfigNameArray[funcIndexs[i]] + "Data(int dataId)\n";
                        codeStr += "\t{\n\t\t" + ConfigNameArray[funcIndexs[i]] + ".Remove(dataId);\n\t}\n";

                        codeStr += "\tpublic " + typeName + " Get" + ConfigNameArray[funcIndexs[i]] + "Data(int dataId)\n";
                        codeStr += "\t{\n\t\t" + typeName + " result = null;\n";
                        codeStr += "\t\t" + ConfigNameArray[funcIndexs[i]] + ".TryGetValue(dataId, out result);\n";
                        codeStr += "\t\treturn result;\n\t}\n";
                    }
                }

            }

        }



        codeStr +="}\n";
        var className = "Data" + ExportName;

        if (!ExportName.Contains("Item"))
        {
            codeStr += "public partial class DataSystem\n{\n\t" + className + " m_" + className + ";\n\n\t";

            codeStr += "public " + className + " Get" + className + "()\n\t{\n\t\t";
            codeStr += "if (m_" + className + " == null)\n\t\t{\n\t\t\t";
            codeStr += "m_" + className + " = new " + className + "();\n\t\t}\n\t\t";
            codeStr += "return m_" + className + ";\n\t}\n\n";

            codeStr += "\tpublic void Save" + className + "()\n\t{\n\t\tSaveJsonFile(\"Data_" + ExportName + "\", m_" + className + ");\n\t}\n";

            codeStr += "\tpublic void Load" + className + "()\n\t{\n\t\tLoadJsonFile(\"Data_" + ExportName + "\", ref m_" + className + ");";
            codeStr += "\n\t}\n}";
        }

        try
        {
                                    StreamWriter sw;
            sw = new StreamWriter(new FileStream(realDiskCSFilePath, FileMode.Create, FileAccess.ReadWrite));

            sw.Write(codeStr);
            sw.Close();
            sw.Dispose();
  
            if (className.Contains("TMP"))
            {
                return;
            }

            Debug.Log(realDiskCSFilePath + " Save Succeed");
            var exPath = Application.dataPath + DataSystemPath;
            var lines = File.ReadAllLines(exPath);
            List<string> list = new List<string>();
            bool isAdded = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(className))
                {
                    isAdded = true;
                    break;
                }
                if (lines[i].Contains("Save"))
                {
                    break;
                }
            }
            if (!isAdded && !ExportName.Contains("Item"))
            {
                
                for (int i = 0; i < lines.Length; i++)
                {
                    list.Add(lines[i]);
                    if (!isAdded && lines[i].Contains("LoadData") && lines[i + 1].Contains("}"))
                    {
                        isAdded = true;
                        list.Add("\t\tLoad" + className + "();");
                        Debug.Log("Add Load InterFace Succed");
                    }
                }
                File.WriteAllLines(exPath, list.ToArray());
            }
                    }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void GenModelCode()
    {
        string realDiskCSFilePath = ModelPath + "/YModel" + ExportName + ".cs";
        string codeStr = "//This File Is Auto Generated By Process Config\n";
        codeStr += "using System.Collections.Generic;\n\npublic class YModel" + ExportName + " : YBaseModel\n{\n";

        for (int i = 0; i < ConfigNameArray.Length; i++)
        {
            codeStr += "\tpublic static uint Prop_" + ConfigNameArray[i] + " = 1 << "+ (i+1) +";\n";
        }

        codeStr += "\tpublic override YModelType GetModelType()\n\t{\n";
        codeStr += "\t\t" + "return YModelType." + ExportName + ";\n";
        codeStr += "\t}\n\n";

        codeStr += "\tpublic override void OnInit()\n\t{\n";
        codeStr += "\t\t" + "base.OnInit();\n";
        codeStr += "\t}\n\n";

        codeStr += "\tpublic override void OnClear()\n\t{\n";
        codeStr += "\t\t" + "base.OnClear();\n";
        codeStr += "\t}\n\n";

        codeStr += "\tpublic override void OnSceneChange()\n\t{\n";
        codeStr += "\t\t" + "base.OnSceneChange();\n";
        codeStr += "\t}\n}";

        try
        {
            StreamWriter sw;
            sw = new StreamWriter(new FileStream(realDiskCSFilePath, FileMode.Create, FileAccess.ReadWrite));

            sw.Write(codeStr);
            sw.Close();
            sw.Dispose();
            Debug.Log(realDiskCSFilePath + " Save Succeed");

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
