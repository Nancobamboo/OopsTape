using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine.U2D;
using System.IO;
using System.Linq;

public class SceneUtil : EditorWindow
{
    public static SceneUtil Instance;
    public float _heightLeft;
    AssetSystem m_Asset;
    ConfigSystem m_Config;
    Dictionary<string, int> m_ConfigArtDict = new Dictionary<string, int>();
    Object m_LastSeceted;

    public enum ESceneName
    {
        GameFlash,
        GameEntry,
        Level01_Chips,
        Level02_Koala,
        Level03_Fly,
        Level04_Sing,
        Level05_Guide,
        Upper
    }

    public int PerColumnNum = 23;

    public static int BatchSpriteSize = 1024;
    public static string AtlasContainName = "Card";

    string m_CurArtKey;
    int m_CurSelectArtId;

    [MenuItem("EditorTools/Scene Util %h")] //Ctrl+H
    public static void Open()
    {
        if (Instance != null)
        {
            Instance.Show();
            return;
        }
        Instance = EditorWindow.GetWindow<SceneUtil>();
        Instance.titleContent = new GUIContent("Scene Util");

        Instance.Show();
    }


    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        _heightLeft = 20;

        EditorGUI.LabelField(GetGUIRect(20, 200, 18, false), "Scene ↓: ");
        _heightLeft += 20;

        for (int j = 0; j < (int)ESceneName.Upper; j++)
        {
            var sceneName = ((ESceneName)j).ToString();
            if (GUI.Button(GetGUIRect(20, 150, 40), sceneName))
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity");
                    m_Asset = null;
                }
            }
        }
        _heightLeft += 20;
        EditorGUI.LabelField(GetGUIRect(20, 200, 18, false), "BatchSpriteSize: ");
        BatchSpriteSize = int.Parse(EditorGUI.TextField(GetGUIRect(120, 60, 18, false), BatchSpriteSize.ToString()));

        //_heightLeft += 20;
        // EditorGUI.LabelField(GetGUIRect(20, 200, 18, false), "AtlasContainName: ");
        // AtlasContainName = EditorGUI.TextField(GetGUIRect(120, 60, 18, false), AtlasContainName);

        _heightLeft += 20;
        EditorGUI.LabelField(GetGUIRect(20, 200, 18, false), "CurSelectArtId: ");

        if (Selection.objects != null && Selection.objects.Length > 0)
        {
            if (m_LastSeceted != Selection.objects[0])
            {
                m_LastSeceted = Selection.objects[0];

                string path = AssetDatabase.GetAssetPath(m_LastSeceted);
                if (path.Contains("Resources/Art"))
                {
                    //var len = "Assets/Resources/Art/".Length;
                    var len = 21;
                    var key = path.Substring(len, path.Length - 25);
                    int artId = 0;
                    m_CurArtKey = key;

                    m_ConfigArtDict.TryGetValue(key, out artId);
                    if (artId != m_CurSelectArtId)
                    {
                        m_CurSelectArtId = artId;
                    }
                }
            }
        }

        EditorGUI.LabelField(GetGUIRect(120, 60, 18, false), m_CurSelectArtId.ToString());
        _heightLeft += 20;

        //if (GUI.Button(GetGUIRect(20, 120, 30), "LoadArtConfig"))
        //{
        //    m_Config = new ConfigSystem();

        //    var list = new List<ConfigArt2D>();
        //    m_Config.LoadJsonConfig("Configs/Config_Art2D",ref list); 
        //    foreach(var config in list)
        //    {
        //        var path = string.Format("{0}/{1}/{2}", config.First, config.Second, config.Real);
        //        m_ConfigArtDict[path] = config.ArtId;
        //    }
        //    Debug.Log("Load Art Config Succeed");
        //}

        _heightLeft = 0;
        EditorGUI.LabelField(GetGUIRect(20, 200, 18, false), "PerColumnNum: ");
        PerColumnNum = int.Parse(EditorGUI.TextField(GetGUIRect(120, 60, 18, false), PerColumnNum.ToString()));

        EditorGUI.LabelField(GetGUIRect(220, 300, 18, false), m_CurArtKey);

        _heightLeft = 20;
        EditorGUI.LabelField(GetGUIRect(190, 200, 18, false), "UI Prefab ↓: ");
        _heightLeft += 20;

        float startHeight = _heightLeft;

        for (int j = 1; j < (int)EResType.UIUpper; j++)
        {
            var prefabName = ((EResType)j).ToString();
            if (prefabName.Contains("UI"))
            {
                int offset = j / PerColumnNum;
                _heightLeft = startHeight + j % PerColumnNum * 30 - 30;
                if (GUI.Button(GetGUIRect(190 + offset * 120, 120, 30), prefabName.Substring(2)))
                {
                    if (!EditorApplication.isPlaying)
                    {
                        if (m_Asset == null)
                        {
                            m_Asset = FindObjectOfType<AssetSystem>();
                        }
                        var resType = (EResType)j;

                        var obj = m_Asset.CreateUIPrefab(resType);
                        var prefab = Resources.Load<GameObject>("UIPrefab/" + resType.ToString());

                        PrefabUtility.ConnectGameObjectToPrefab(obj, prefab);
                    }
                }
            }
        }
    }

    private Rect GetGUIRect(float leftPos, float width, float height, bool addHeight = true)
    {
        Rect rect = new Rect(leftPos, _heightLeft, width, height);
        if (addHeight)
        {
            _heightLeft += height;
        }
        return rect;
    }

    [MenuItem("Assets/MyEditor/Set Directory Sprites")]
    public static void SetDirectorySprites()
    {
        if (Selection.objects.Length > 0)
        {
            string selectionPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
            DirectoryInfo direction = new DirectoryInfo(selectionPath);
            DirectoryInfo[] allDirs = direction.GetDirectories();
            var resStr = "Resources";
            for (int i = 0; i < allDirs.Length; i++)
            {
                var dir = allDirs[i];
                var childDirs = dir.GetDirectories();

                foreach (var childDir in childDirs)
                {
                    var index = childDir.FullName.LastIndexOf(resStr);
                    var path = childDir.FullName.Substring(index + resStr.Length + 1);
                    var files = Resources.LoadAll(path);
                    foreach (var file in files)
                    {
                        SetTexturePropty(AssetDatabase.GetAssetPath(file));
                    }
                }
            }
        }
    }

    static void SetTexturePropty(Texture texture)
    {
        string selectionPath = AssetDatabase.GetAssetPath(texture);
        TextureImporter textureIm = AssetImporter.GetAtPath(selectionPath) as TextureImporter;
        textureIm.spritePixelsPerUnit = 72;
        textureIm.textureType = TextureImporterType.Sprite;
        textureIm.spriteImportMode = SpriteImportMode.Single;
        textureIm.isReadable = false;
        var setting = textureIm.GetDefaultPlatformTextureSettings();
        setting.format = TextureImporterFormat.Automatic;
        setting.compressionQuality = 2;
        setting.maxTextureSize = BatchSpriteSize;

        textureIm.SetPlatformTextureSettings(setting);

        AssetDatabase.ImportAsset(selectionPath);
    }

    static void SetTexturePropty(string selectionPath)
    {
        TextureImporter textureIm = AssetImporter.GetAtPath(selectionPath) as TextureImporter;
        textureIm.spritePixelsPerUnit = 72;
        textureIm.textureType = TextureImporterType.Sprite;
        textureIm.spriteImportMode = SpriteImportMode.Single;
        textureIm.isReadable = false;
        var setting = textureIm.GetDefaultPlatformTextureSettings();
        setting.format = TextureImporterFormat.Automatic;
        setting.compressionQuality = 2;
        setting.maxTextureSize = BatchSpriteSize;

        textureIm.SetPlatformTextureSettings(setting);

        AssetDatabase.ImportAsset(selectionPath);
    }


    [MenuItem("Assets/MyEditor/Batch Set Sprite")]
    public static void ExchageSprite()
    {
        if (Selection.objects.Length > 0)
        {
            foreach (Texture texture in Selection.objects)
            {
                SetTexturePropty(texture);
            }
        }
    }


    [MenuItem("Assets/MyEditor/Create Atlas")]
    public static void CreateAtlas()
    {
        string atlasRoot = "Assets/Resources/Atlas/";

        if (!Directory.Exists(atlasRoot))
        {
            Directory.CreateDirectory(atlasRoot);
        }
        if (Selection.objects.Length > 0)
        {
            string selectPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
            var dir = selectPath.Split('/');


            SpriteAtlas altas = GetAtlas();
            altas.Add(Selection.objects);
            var atlasName = atlasRoot + dir[dir.Length - 2] + ".spriteatlas";
            AssetDatabase.CreateAsset(altas, atlasName);
            AssetDatabase.SaveAssets();
            Debug.Log("图集创建:" + atlasName);
        }
    }

    [MenuItem("Assets/MyEditor/Batch Create Atlas")]
    static void BatchAtlasCreate()
    {
        string artRoot = AssetDatabase.GetAssetPath(Selection.objects[0]);
        Debug.Log(artRoot);
        //string artRoot = "Assets/Resources/Art";
        string atlasRoot = "Assets/Resources/Atlas";

        DirectoryInfo direction = new DirectoryInfo(artRoot);
        DirectoryInfo[] allDirs = direction.GetDirectories();//文件夹


        for (int i = 0; i < allDirs.Length; i++)
        {
            var dir = allDirs[i];

            var atlasName = atlasRoot + "/" + dir.Name + ".spriteatlas";

            //if(!atlasName.Contains(AtlasContainName))
            //{
            //    continue;
            //}
            //创建图集
            if (File.Exists(atlasName))
            {
                File.Delete(atlasName);
            }

            SpriteAtlas altas = GetAtlas();

            var subPath = artRoot.Substring("Assets/Resources/".Length);
            //Debug.LogError(subPath);


            var sprites = Resources.LoadAll<Sprite>(subPath + "/" + dir.Name);
            altas.Add(sprites);
            AssetDatabase.CreateAsset(altas, atlasName);
            AssetDatabase.SaveAssets();
            Debug.Log("图集创建:" + atlasName);
        }
    }

    static SpriteAtlas GetAtlas()
    {
        SpriteAtlas altas = new SpriteAtlas();

        SpriteAtlasPackingSettings packSet = new SpriteAtlasPackingSettings()
        {
            blockOffset = 1,
            enableRotation = false,
            enableTightPacking = true,
            enableAlphaDilation = true,
            padding = 8,
        };
        altas.SetPackingSettings(packSet);


        SpriteAtlasTextureSettings textureSet = new SpriteAtlasTextureSettings()
        {
            readable = false,
            generateMipMaps = false,
            sRGB = true,
            filterMode = FilterMode.Bilinear,

        };
        altas.SetTextureSettings(textureSet);

        var platformSetting = altas.GetPlatformSettings("Default");
        platformSetting.maxTextureSize = BatchSpriteSize;

        return altas;
    }

    [MenuItem("Tools/Start Game %g")] //Ctrl+G  

    public static void StartGame()
    {
        if (EditorUtility.DisplayDialog("Start game",
            "Do you want to run game anyway? \n\nAll unsaved datas in current scene will be LOST!!!!!!", "GoGoGo!!!", "No") == false)
        {
            return;
        }

        EditorApplication.isPaused = false;
        EditorApplication.isPlaying = false;
        EditorSceneManager.OpenScene("Assets/Scenes/RealGame.unity");
        EditorApplication.isPlaying = true;
    }
}
