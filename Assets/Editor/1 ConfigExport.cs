using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

public class ConfigExport
{
    [MenuItem("EditorTools/Config Export")]
    private static void Export()
    {
        string sourcePath = Application.dataPath + "/../Configs/";
        string exportPath = Application.dataPath + "/Resources/Configs/";

        Debug.Log("----------start export config----------");

        string[] excelList = Directory.GetFiles(sourcePath, "*.xlsx");

        foreach (string excelPath in excelList)
        {
            if (excelPath.Contains("Localization.xlsx"))
                continue;

            if(excelPath.Contains("Test"))
            {
                continue;
            }

            Debug.Log("export: " + excelPath);
            ExcelUtility excel = new ExcelUtility(excelPath);

            Encoding encoding = Encoding.GetEncoding("utf-8");
            string output = exportPath + Path.GetFileNameWithoutExtension(excelPath) + ".json";
            excel.ConvertToJson(output, encoding);
        }

        Debug.Log("----------end export config----------");
        //EditorUtility.DisplayDialog("Hi", "finish export config.", "ok");
        AssetDatabase.Refresh();
    }

    [MenuItem("EditorTools/Get Special Excel")]
    public static void GetSpecialExcel()
    {
        string artRoot = "Assets/Resources/Art";
        string exportName = "ConfigArt.csv";

        GetRoleExcel(artRoot, exportName);
    }


    public static void GetRoleExcel(string artRoot, string exportName)
    {
        string sourcePath = Application.dataPath + "/../Configs/";

        DirectoryInfo direction = new DirectoryInfo(artRoot);
        DirectoryInfo[] allDirs = direction.GetDirectories();
        List<string> resultList = new List<string>();

        int startId = 1;
        foreach (var dir in allDirs)
        {
            var childDirs = dir.GetDirectories();

            foreach (var childDir in childDirs)
            {
                var files = childDir.GetFiles();

                //var files = dir.GetFiles();
                foreach (var file in files)
                {
                    if (file.Name.Contains("meta"))
                    {
                        continue;
                    }

                    var line = startId + "," + dir.Name + ',' + childDir.Name + ',' + file.Name;
                    //var line = startId + "," + dir.Name + ',' + dir.Name + ',' + file.Name;
                    startId++;
                    resultList.Add(line);
                }
            }
        }
        File.WriteAllLines(sourcePath + exportName, resultList.ToArray());
    }

    //[MenuItem("EditorTools/Get ExcelValue")]
    public static void FillExcelValue()
    {
        string sourcePath = Application.dataPath + "/../Configs/";
        var cityPath = sourcePath + "Config_Item.xlsx";
        var artPath = sourcePath + "Test_物品.xlsx";

        var cityExcel = new ExcelUtility(cityPath);
        var artExcel = new ExcelUtility(artPath);
        var citySheet = cityExcel.GetDataSheet();
        var artSheet = artExcel.GetDataSheet();
        var result = "";
        for (int j = 1; j < citySheet.Rows.Count; j++)
        {
            var city = citySheet.Rows[j][1].ToString();
            bool hasResult = false;
            for (int i = 1; i < artSheet.Rows.Count; i++)
            {
                //var id = int.Parse(artSheet.Rows[i][0].ToString());
                //if (id < 1408)
                //{
                //    continue;
                //}

                var row = artSheet.Rows[i][3].ToString();

                if (row != null && city != null && row == city)
                {
                    result += artSheet.Rows[i][10] + "-";
                    hasResult = true;
                    break;
                }
            }
            if (!hasResult)
            {
                result += 0 + "-";
            }
        }

        Debug.Log(result);
    }

    //[MenuItem("EditorTools/Get Multi ExcelValue")]
    public static void FillMultiExcelValue()
    {
        string sourcePath = Application.dataPath + "/../Configs/";
        var rolePath = sourcePath + "Config_EduRoleLike.xlsx";
        var cardPath = sourcePath + "Test_物品.xlsx";

        var roleExcel = new ExcelUtility(rolePath);
        var cardExcel = new ExcelUtility(cardPath);
        var roleSheet = roleExcel.GetDataSheet();
        var cardSheet = cardExcel.GetDataSheet();
        var result = "";
        for (int j = 1; j < roleSheet.Rows.Count; j++)
        {
            var roleCards = roleSheet.Rows[j][6].ToString();
            bool hasResult = false;
            for (int i = 1; i < cardSheet.Rows.Count; i++)
            {
                var cardName = cardSheet.Rows[i][6].ToString();

                if (cardName != null && roleCards != null && roleCards.Contains(cardName))
                {
                    result += cardSheet.Rows[i][3] + "|";
                    hasResult = true;
                }
            }
            if (!hasResult)
            {
                result += roleCards + "-";
            }
            else
            {
                result += "-";
            }
        }

        Debug.Log(result);

        //foreach(var citySheet)
    }

    //[MenuItem("EditorTools/Get OneExcelValue")]
    public static void GetOneExcelValue()
    {
        string sourcePath = Application.dataPath + "/../Configs/";
        var cityPath = sourcePath + "Config_EduCity.xlsx";

        var cityExcel = new ExcelUtility(cityPath);
        var citySheet = cityExcel.GetDataSheet();
        var result = "";
        for (int j = 1; j < citySheet.Rows.Count; j++)
        {
            bool hasResult = false;
            var cityCards = citySheet.Rows[j][2].ToString().Split('|');
            for (int i = 0; i < cityCards.Length; i++)
            {
                int num = 0;
                int.TryParse(cityCards[i], out num);
                if (num > 0)
                {
                    hasResult = true;
                    result+= (i+1)+ "|";
                }
            }
            if (!hasResult)
            {
                result += 0 + "-";
            }
            else
            {
                result += "-";
            }
        }
        Debug.Log(result);

    }

    public string Para = "28|40|40|45|45|60|35|35|30|30|60|60|58|50";
    public int Multi = 2;


    [ContextMenu("GetParaValue")]
    public void GetParaValue()
    {
        var array = Para.Split('|');

        var result = "";

        for (int i = 0; i < array.Length; i++)
        {
            result += int.Parse(array[i]) * Multi;

            if (i != array.Length - 1)
            {
                result += "\n";
            }
        }

        Debug.Log(result);
    }
}
