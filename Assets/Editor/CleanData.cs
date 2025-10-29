using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CleanData
{
    [MenuItem("Tools/CleanData")]
    private static void Clean()
    {
        var savePath = Application.dataPath + "/Datas/";
        if (string.IsNullOrEmpty(savePath))
            savePath = Application.persistentDataPath + "/";

        string[] jfiles = Directory.GetFiles(savePath, "*.json");
        string[] metafiles = Directory.GetFiles(savePath, "*.json.meta");

        foreach (string jfile in jfiles)
        {
            if (File.Exists(jfile))
            {
                try
                {
                    File.Delete(jfile);
                }
                catch (IOException e)
                {
                    System.Console.WriteLine(e.Message);
                    EditorUtility.DisplayDialog("Hi", "fail clean.", "ok");
                    return;
                }
            }
        }

        foreach (string jfile in metafiles)
        {
            if (File.Exists(jfile))
            {
                try
                {
                    File.Delete(jfile);
                }
                catch (IOException e)
                {
                    System.Console.WriteLine(e.Message);
                    EditorUtility.DisplayDialog("Hi", "fail clean.", "ok");
                    return;
                }
            }
        }

        PlayerPrefs.DeleteAll();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Hi", "success clean.", "ok");
    }
}
