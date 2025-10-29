using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine.UI;

public class YProcessViewEditor : EditorWindow
{
    public static YProcessViewEditor Instance;
    public Rect WindowRect;
    public Rect ContentRect;
    public float LeftWidth = 200f;
    public float TopHeight = 10f;
    public float TitleHeight = 10f;
    public float HeightLeft;
    public bool IsShowExtra;

    YViewItem m_CurItemVar;
    YViewReference m_CurItemRef;
    GameObject m_CurRoot;

    public UnityEngine.Object LastSelectObject;
    const string ViewCodePath = "Assets/Scripts/ViewControl";
    const string ControlCodePath = "Assets/Scripts/ViewControl";
    string PrefabPath = "Assets/Resources/UIPrefab";
    string LevelPrefabPath = "Assets/Resources/Level";

    public bool IsPrefabMode;
    public bool IsRunningAll = false;
    public int PosA;
    public int PosB;
    public int PerColumnNum = 23;
    public string PSBPath = "Assets/ArtStatic";

    public string ResPrefabPath = "VFX";
    public bool ResPrefabConnect;

    [MenuItem("EditorTools/Process View  %q")] //Ctrl+Q
    public static void Open()
    {
        if (Instance != null)
        {
            Instance.Show();
            return;
        }
        Instance = EditorWindow.GetWindow<YProcessViewEditor>();
        Instance.titleContent = new GUIContent("Process View Editor");

        Instance.Show();
    }
    public GameObject GetRootGameObject(GameObject obj)
    {
        Transform result = null;
        result = obj.transform;
        while (result.parent != null)
        {
            if (result.GetComponent<YViewReference>())
            {
                break;
            }

            if (result.parent.name.Contains("Canvas"))
            {
                break;
            }

            if (result.parent.name.Contains("Context"))
            {
                break;
            }

            result = result.parent;
        }
        return result.gameObject;
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPaused)
        {
            return;
        }

        var selectObject = Selection.objects;
        if (selectObject.Length == 0 || IsRunningAll)
        {
            return;
        }

        if (Instance == null)
        {
            Instance = this;
        }

        if (LastSelectObject != selectObject[0])
        {
            LastSelectObject = selectObject[0];
            IsPrefabMode = UnityEditor.SceneManagement.EditorSceneManager.IsPreviewSceneObject(selectObject[0]);

            if (IsPrefabMode)
            {
                m_CurRoot = GetRootGameObject(selectObject[0] as GameObject);
            }
            else
            {
                m_CurRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(selectObject[0]);
            }
        }

        if (m_CurRoot != null)
        {
            var curSelect = (selectObject[0] as GameObject);

            m_CurItemRef = m_CurRoot.GetComponent<YViewReference>();
            bool isExist = m_CurItemRef != null && m_CurItemRef.IsExistItem(curSelect.transform);

            if (isExist)
            {
                m_CurItemVar = m_CurItemRef.GetItem(curSelect.transform);
            }
            else
            {
                HeightLeft = 20f;
                if (GUI.Button(GetGUIRect(20, 150, 18), "Add View Item"))
                {
                    if (m_CurItemRef == null)
                    {
                        m_CurItemRef = m_CurRoot.AddComponent<YViewReference>();
                    }

                    m_CurItemVar = m_CurItemRef.GetItem((selectObject[0] as GameObject).transform);
                    m_CurItemVar.Components = new Object[] { selectObject[0] as GameObject };
                }

                HeightLeft += 10;
                if (GUI.Button(GetGUIRect(20, 150, 18), "ReadGuide"))
                {
                    //ProcessAllPrefabByFileList();
                    //WWW a = new WWW("http://github.com");
                    //Application.OpenURL(a.url);
                }

                if (m_CurItemRef != null)
                {
                    HeightLeft += 10;
                    if (GUI.Button(GetGUIRect(20, 150, 18), "ExportView"))
                    {
                        GenCSCodeFile();
                    }

                    HeightLeft += 40;

                    IsShowExtra = EditorGUI.Toggle(GetGUIRect(20, 260, 18), "More Func", IsShowExtra);

                    if (IsShowExtra)
                    {
                        EditorGUI.LabelField(GetGUIRect(20, 200, 18, false), "ViewItemCount is: " + m_CurItemRef.ViewItemList.Count.ToString());
                        HeightLeft += 20;


                        PosA = int.Parse(EditorGUI.TextField(GetGUIRect(40, 50, 18, false), PosA.ToString()));
                        PosB = int.Parse(EditorGUI.TextField(GetGUIRect(100, 50, 18, false), PosB.ToString()));
                        HeightLeft += 20;

                        if (GUI.Button(GetGUIRect(20, 150, 18), "ExchangeItem"))
                        {
                            m_CurItemRef.ExchangeItemPos(PosA, PosB);
                        }

                        HeightLeft += 10;
                        if (GUI.Button(GetGUIRect(20, 150, 18), "MoveItemToTarget"))
                        {
                            m_CurItemRef.MoveToTargetPos(PosA, PosB);
                        }

                        HeightLeft += 10;
                        if (GUI.Button(GetGUIRect(20, 150, 18), "Export Control"))
                        {
                            GenControlCode();
                        }
                    }
                }
                HeightLeft += 10;

                EditorGUI.LabelField(GetGUIRect(20, 200, 18, false), "Batch Quick Start ↓");

                HeightLeft += 10;

                for (int i = 0; i < UITypeNames.Length; i++)
                {
                    HeightLeft += 10;
                    if (GUI.Button(GetGUIRect(20, 150, 18), UITypeNames[i]))
                    {
                        if (m_CurItemRef == null)
                        {
                            m_CurItemRef = m_CurRoot.AddComponent<YViewReference>();
                        }

                        for (int j = 0; j < selectObject.Length; j++)
                        {
                            m_CurItemVar = m_CurItemRef.GetItem((selectObject[j] as GameObject).transform);
                            var tmpComp = (selectObject[j] as GameObject).GetComponent(UITypeNames[i]);
                            if (tmpComp != null)
                            {
                                m_CurItemVar.Components = new Object[] { tmpComp };
                            }
                            else
                            {
                                m_CurItemVar.Components = new Object[] { selectObject[j] as GameObject };
                            }
                        }
                    }
                }
                return;
            }


            WindowRect = new Rect(0, 0, position.width, position.height);
            ContentRect = new Rect(LeftWidth, TopHeight, position.width, position.height);

            //EditorGUI.LabelField(new Rect(5, 20, WindowRect.xMax, TitleHeight), new GUIContent(m_CurRoot.name + " -> " + selectObject[0].name));

            GUI.Box(new Rect(5, Instance.TopHeight, Instance.LeftWidth, Instance.WindowRect.height), "");

            System.Type type = m_CurItemVar.GetType();
            FieldInfo[] fileds = type.GetFields();

            HeightLeft = TopHeight;

            for (int j = 1; j < fileds.Length - 1; j++)
            {
                FieldInfo filed = fileds[j];
                HeightLeft += 5;

                Vector2 size = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).label.CalcSize(new GUIContent(filed.Name));
                EditorGUI.LabelField(GetGUIRect(10, size.x, size.y, size.x > 100), filed.Name);
                Rect rect = GetGUIRect(60, 150, 18);
                DrawNormalValue(filed, rect, m_CurItemVar);
            }

            HeightLeft += 10;
            if (GUI.Button(GetGUIRect(20, 150, 18), "+"))
            {
                GUIUtility.keyboardControl = 0;
                if (m_CurItemVar.Components == null)
                {
                    m_CurItemVar.Components = new Component[0];
                }
                Array.Resize<string>(ref m_CurItemVar.VarNameArray, m_CurItemVar.Components.Length + 1);
                Array.Resize<Object>(ref m_CurItemVar.Components, m_CurItemVar.Components.Length + 1);
            }

            HeightLeft += 10;
            if (GUI.Button(GetGUIRect(20, 150, 18), "-"))
            {
                GUIUtility.keyboardControl = 0;
                Array.Resize<string>(ref m_CurItemVar.VarNameArray, m_CurItemVar.Components.Length - 1);
                Array.Resize<Object>(ref m_CurItemVar.Components, m_CurItemVar.Components.Length - 1);
            }

            HeightLeft += 60;
            if (GUI.Button(GetGUIRect(20, 150, 18), "Reset"))
            {
                GUIUtility.keyboardControl = 0;
                Array.Resize<string>(ref m_CurItemVar.VarNameArray, 0);
                Array.Resize<Object>(ref m_CurItemVar.Components, 0);
            }

            HeightLeft += 10;
            if (GUI.Button(GetGUIRect(20, 150, 18), "Remove This View Item"))
            {
                GUIUtility.keyboardControl = 0;
                //m_CurItemRef.RemoveItem(m_CurItemVar);

                for (int i = 0; i < selectObject.Length; i++)
                {
                    m_CurItemRef.RemoveItem(m_CurItemRef.GetItem(((GameObject)selectObject[i]).transform));
                }

                m_CurItemVar = null;
            }


            HeightLeft += 10;
            if (GUI.Button(GetGUIRect(20, 150, 18), "Export View"))
            {
                GenCSCodeFile();
            }


            IsShowExtra = EditorGUI.Toggle(GetGUIRect(20, 260, 18), "More Func", IsShowExtra);

            if (IsShowExtra)
            {
                EditorGUI.LabelField(GetGUIRect(20, 200, 18, false), "ViewItemCount is: " + m_CurItemRef.ViewItemList.Count.ToString());
                HeightLeft += 20;
                HeightLeft += 10;
                if (GUI.Button(GetGUIRect(20, 150, 18), "Export Control"))
                {
                    GenControlCode();
                }
                HeightLeft += 10;
                if (GUI.Button(GetGUIRect(10, 180, 18), "Remove All Child Reference"))
                {
                    RemoveAllReference();
                }
            }

            GUI.Box(new Rect(210, Instance.TopHeight, Instance.LeftWidth, Instance.WindowRect.height), "");
            //_heightLeft = _topHeight + 22;
            HeightLeft = TopHeight;

            for (int j = fileds.Length - 1; j < fileds.Length; j++)
            {
                FieldInfo filed = fileds[j];
                HeightLeft += 5;

                Vector2 size = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).label.CalcSize(new GUIContent(filed.Name));
                EditorGUI.LabelField(GetGUIRect(220, size.x, size.y, size.x > 100), filed.Name);
                Rect rect = GetGUIRect(100, 150, 18);
                DrawNormalValue(filed, rect, m_CurItemVar);
            }

            HeightLeft += 10;

            EditorGUI.LabelField(GetGUIRect(220, 200, 18), "Quick Fill Component Value ↓");

            var comps = curSelect.GetComponents<Component>();

            foreach (var comp in comps)
            {
                HeightLeft += 10;
                var compType = comp.GetType().Name;

                if (GUI.Button(GetGUIRect(230, 150, 18), compType))
                {
                    string name = GUI.GetNameOfFocusedControl();
                    if (name.Contains("Component"))
                    {
                        var index = name.Substring(10);
                        switch (compType)
                        {
                            case "Transform":
                                m_CurItemVar.Components[int.Parse(index)] = m_CurItemVar.Target;
                                break;
                            default:
                                m_CurItemVar.Components[int.Parse(index)] = m_CurItemVar.Target.GetComponent(compType);
                                break;
                        }
                    }
                    GUIUtility.keyboardControl = 0;
                    if (IsPrefabMode)
                    {
                        EditorUtility.SetDirty(m_CurRoot);
                    }
                }
            }

            HeightLeft += 10;

            if (GUI.Button(GetGUIRect(230, 150, 18), "GameObject"))
            {
                string name = GUI.GetNameOfFocusedControl();
                if (name.Contains("Component"))
                {
                    var index = name.Substring(10);
                    m_CurItemVar.Components[int.Parse(index)] = m_CurItemVar.Target.gameObject;
                }
                GUIUtility.keyboardControl = 0;
            }
        }
        else
        {
            HeightLeft = 20f;

            if (GUI.Button(GetGUIRect(20, 150, 18, false), "Create Prefab"))
            {
                var obj = LastSelectObject as GameObject;
                if (obj == null)
                {
                    return;
                }
                var targetDir = obj.name.Contains("Level") ? LevelPrefabPath : PrefabPath;
                var targetPath = targetDir + "/" + obj.name + ".prefab";
                PrefabUtility.SaveAsPrefabAssetAndConnect(obj, targetPath, InteractionMode.UserAction);

            }
            HeightLeft += 40;
            EditorGUI.LabelField(GetGUIRect(20, 200, 18, false), "PerColumnNum: ");
            PerColumnNum = int.Parse(EditorGUI.TextField(GetGUIRect(120, 60, 18, false), PerColumnNum.ToString()));

            HeightLeft += 40;
            EditorGUI.LabelField(GetGUIRect(20, 160, 18, false), "PSBPath: ");
            HeightLeft += 20;
            PSBPath = EditorGUI.TextField(GetGUIRect(20, 160, 18, false), PSBPath.ToString());

            HeightLeft += 40;
            EditorGUI.LabelField(GetGUIRect(20, 160, 18, false), "ResourcePath: ");
            HeightLeft += 20;
            ResPrefabPath = EditorGUI.TextField(GetGUIRect(20, 160, 18, false), ResPrefabPath.ToString());

            HeightLeft += 20;
            ResPrefabConnect = EditorGUI.Toggle(GetGUIRect(20, 160, 18, false), false);

            DirectoryInfo direction = new DirectoryInfo("Assets/Resources/" + ResPrefabPath);
            FileInfo[] allFiles = null;
            if (direction.Exists)
            {
                allFiles = direction.GetFiles();
            }

            if (allFiles == null || allFiles.Length == 0)
            {
                direction = new DirectoryInfo(PSBPath);
                if (!direction.Exists)
                {
                    return;
                }

                allFiles = direction.GetFiles();
                HeightLeft = 20f;
                EditorGUI.LabelField(GetGUIRect(200, 200, 18, false), "PSB Res List: ");
                // string result = null; // unused

                int assetIndex = 0;
                for (int j = 0; j < allFiles.Length; j++)
                {
                    var fileName = allFiles[j];
                    int offset = assetIndex / PerColumnNum;
                    HeightLeft = 80 + assetIndex % PerColumnNum * 30 - 30;
                    if (!fileName.Name.Contains("meta") && fileName.Name.Contains(".psb"))
                    {
                        //result += fileName.Name + "\n";

                        assetIndex++;
                        if (GUI.Button(GetGUIRect(190 + offset * 120, 120, 30), fileName.Name))
                        {
                            //Debug.Log(result);
                            var obj = LastSelectObject as GameObject;

                            var prefab = AssetDatabase.LoadAssetAtPath(PSBPath + "/" + fileName.Name, typeof(GameObject));
                            GameObject entity = GameObject.Instantiate(prefab, obj.transform) as GameObject;
                            ExchangePSBPrefab(entity);
                        }
                    }

                }
            }
            else
            {
                HeightLeft = 20f;
                EditorGUI.LabelField(GetGUIRect(200, 200, 18, false), "Res Prefab List: ");
                // string result = null; // unused

                int assetIndex = 0;
                for (int j = 0; j < allFiles.Length; j++)
                {
                    var fileName = allFiles[j];
                    int offset = assetIndex / PerColumnNum;
                    HeightLeft = 80 + assetIndex % PerColumnNum * 30 - 30;
                    if (!fileName.Name.Contains("meta") && fileName.Name.Contains(".prefab"))
                    {
                        //result += fileName.Name + "\n";

                        assetIndex++;
                        var prefabName = fileName.Name.Substring(0, fileName.Name.Length - 7);
                        if (GUI.Button(GetGUIRect(190 + offset * 120, 120, 30), prefabName))
                        {
                            var obj = LastSelectObject as GameObject;
                            var prefab = Resources.Load<GameObject>(ResPrefabPath + "/" + prefabName);
                            GameObject entity = GameObject.Instantiate(prefab, obj.transform);
                            if (ResPrefabConnect)
                            {
                                // PrefabUtility.ConnectGameObjectToPrefab(entity, prefab); // deprecated

                            }

                        }
                    }

                }
            }



        }
    }


    public void ExchangePSBPrefab(GameObject entity)
    {
        var child = entity.GetComponentsInChildren<SpriteRenderer>();
        var root = entity.transform.parent;
        for (int i = 0; i < child.Length; i++)
        {
            //Debug.Log(imgs[i].transform.GetSiblingIndex());
            child[i].transform.SetParent(root);
        }
        DestroyImmediate(entity.gameObject);

        for (int i = 0; i < child.Length; i++)
        {
            var trans = child[i].gameObject;
            var image = trans.AddComponent<Image>();
            image.sprite = child[i].sprite;
            image.color = child[i].color;
            image.SetNativeSize();
        }

        for (int i = 0; i < child.Length; i++)
        {
            DestroyImmediate(child[i]);
        }

        var imgs = root.GetComponentsInChildren<Image>();
        for (int i = 0; i < imgs.Length; i++)
        {
            //Debug.Log(imgs[i].transform.GetSiblingIndex());
            imgs[i].transform.SetSiblingIndex(imgs.Length - i);
            var pos = imgs[i].transform.localPosition * 100;
            pos.y -= 540;
            imgs[i].transform.localPosition = pos;

        }
    }

    public void RemoveAllReference()
    {
        var childs = m_CurItemVar.Target.GetComponentsInChildren<Transform>(true);

        var removeList = new List<YViewItem>();
        foreach (var item in m_CurItemRef.ViewItemList)
        {
            foreach (var child in childs)
            {
                if (child == item.Target)
                {
                    removeList.Add(item);
                }
            }
        }

        foreach (var item in removeList)
        {
            m_CurItemRef.ViewItemList.Remove(item);
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

    string[] UITypeNames = new string[] {
         "Button",
          "Image",
          "Text",
          "RectTransform",
          "Slider",
    };


    public bool IsUITypeName(string typeName)
    {
        int index = Array.IndexOf(UITypeNames, typeName);
        if (index >= 0 && index < UITypeNames.Length)
        {
            return true;
        }
        return false;
    }

    public bool IsDoTweenName(string typeName)
    {
        switch (typeName)
        {
            case "DOTweenAnimation":
                return true;
        }
        return false;
    }

    public bool IsTextMeshName(string typeName)
    {
        switch (typeName)
        {
            case "TextMeshProUGUI":
                return true;
        }
        return false;
    }

    public bool IsUIParticle(string typeName)
    {
        return typeName.Contains("UIParticle");
    }


    public string GetVarNameByDict(string key, int compIndex, YViewItem item)
    {
        var result = "";

        if (string.IsNullOrEmpty(result))
        {
            if (item.VarNameArray.Length > compIndex)
            {
                if (string.IsNullOrEmpty(item.VarNameArray[compIndex]))
                {
                    return item.Target.name;
                }
                else
                {
                    return item.VarNameArray[compIndex];
                }
            }
        }

        if (string.IsNullOrEmpty(result))
        {
            if (item.Components.Length == 1)
            {
                return item.Target.name;
            }
            else
            {
                return key;
            }
        }

        return result;
    }

    private void DrawNormalValue(System.Reflection.FieldInfo filed, Rect rect, object o)
    {
        switch (filed.FieldType.ToString())
        {
            case "UnityEngine.Object[]":
                Object[] objArray = filed.GetValue(o) as Object[];
                if (objArray == null)
                {
                    return;
                }

                List<string> compArray = new List<string>();
                foreach (var comp in objArray)
                {
                    if (comp == null)
                    {
                        compArray.Add("");
                    }
                    else
                    {
                        compArray.Add(comp.GetType().Name);
                    }
                }
                for (int j = 0; j < compArray.Count; j++)
                {
                    GUI.SetNextControlName(filed.Name + j);
                    rect = GetGUIRect(10, 180, 18, false);
                    compArray[j] = EditorGUI.TextField(rect, (string)compArray[j]);
                    GetGUIRect(175, 20, 18);
                    HeightLeft += 2;
                }
                //filed.SetValue(o, objArray);

                break;
            case "UnityEngine.Transform":
                Transform target = (Transform)filed.GetValue(o);
                EditorGUI.LabelField(rect, target == null ? "Notice: Is NULL!" : target.name);
                break;
            case "System.String":
                var strRect = GetGUIRect(10, rect.width, 20, true);
                filed.SetValue(o, EditorGUI.TextField(strRect, (string)filed.GetValue(o)));
                break;
            case "System.String[]":
                if (filed == null)
                {
                    break;
                }
                string[] array = filed.GetValue(o) as string[];
                if (array == null)
                    array = new string[] { };
                for (int j = 0; j < array.Length; j++)
                {
                    GUI.SetNextControlName(filed.Name + j);
                    rect = GetGUIRect(220, 180, 18, false);
                    array[j] = EditorGUI.TextField(rect, (string)array[j]);
                    GetGUIRect(175, 20, 18);
                    HeightLeft += 2;
                }
                filed.SetValue(o, array);
                break;

            default:
                if (filed.FieldType.BaseType.ToString() == "System.Enum")
                {
                    filed.SetValue(o, EditorGUI.EnumPopup(rect, (System.Enum)filed.GetValue(o)));
                }
                else
                {
                    EditorGUI.LabelField(rect, "Not Deal Object.");
                }
                break;
        }
    }

    private void GenCSCodeFile()
    {
        GenViewCodeFile();
    }

    private void GenViewCodeFile()
    {
        GameObject prefab = m_CurItemRef.gameObject;
        var newPrefab = m_CurItemRef.gameObject;
        if (!IsPrefabMode)
        {
            prefab = PrefabUtility.GetCorrespondingObjectFromSource(m_CurItemRef.gameObject);
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
            bool isSucceed = false;
            PrefabUtility.SaveAsPrefabAsset(newPrefab, path, out isSucceed);
            Debug.Log(path + " Save Succeed：" + isSucceed);
        }
        else if (IsRunningAll)
        {
            PrefabUtility.SavePrefabAsset(m_CurItemRef.gameObject);
        }
        else if (IsPrefabMode)
        {
            EditorUtility.SetDirty(prefab);
        }

        m_CurItemRef = prefab.GetComponent<YViewReference>();

        AssetDatabase.Refresh();

        string realDiskCSFilePath = ViewCodePath + "/" + Path.GetFileNameWithoutExtension(prefab.name) + "View.cs";

        string codeStr = "//This File Is Auto Generated By Process View\n";

        bool isUIView = false;
        bool isDoTween = false;
        bool isTextMesh = false;
        bool isUIParticle = false;
        if (m_CurItemRef.ViewItemList.Count > 0)
        {
            foreach (var item in m_CurItemRef.ViewItemList)
            {
                for (int i = 0; i < item.Components.Length; i++)
                {
                    var typeName = item.Components[i].GetType().Name;
                    if (IsUITypeName(typeName))
                    {
                        isUIView = true;
                    }
                    else if (IsDoTweenName(typeName))
                    {
                        isDoTween = true;
                    }
                    else if (IsTextMeshName(typeName))
                    {
                        isTextMesh = true;
                    }
                    else if (IsUIParticle(typeName))
                    {
                        isUIParticle = true;
                    }
                }
            }
        }

        codeStr += "using UnityEngine;\n";

        if (isUIView)
        {
            codeStr += "using UnityEngine.UI;\n\n";
        }


        if (isDoTween)
        {
            codeStr += "using DG.Tweening;\n\n";
        }

        if (isTextMesh)
        {
            codeStr += "using TMPro;\n\n";
        }

        if (isUIParticle)
        {
            codeStr += "using Coffee.UIExtensions;\n\n";
        }

        if (!isUIView && !isDoTween && !isTextMesh)
        {
            codeStr += "\n";
        }

        codeStr += "public class " + Path.GetFileNameWithoutExtension(prefab.name) + "View : YBaseView\n{\n";

        if (m_CurItemRef.ViewItemList.Count > 0)
        {
            foreach (var item in m_CurItemRef.ViewItemList)
            {
                for (int i = 0; i < item.Components.Length; i++)
                {
                    var typeName = item.Components[i].GetType().Name;

                    var targetName = GetVarNameByDict(typeName + item.Target.name, i, item);

                    codeStr += "\tpublic " + typeName + " " + targetName + ";\n";
                }
            }

            codeStr += "\tpublic override void OnInit(Transform holder)\n\t{\n";
            codeStr += "\t\tvar itemRef = holder.GetComponent<YViewReference>();\n";
            codeStr += "\t\tif(itemRef == null) return;\n";
            codeStr += "\t\tvar viewItemList = itemRef.ViewItemList;\n";
            codeStr += "\t\tif(viewItemList == null || viewItemList.Count == 0) return;\n";

            for (int i = 0; i < m_CurItemRef.ViewItemList.Count; i++)
            {
                var item = m_CurItemRef.ViewItemList[i];

                for (int j = 0; j < item.Components.Length; j++)
                {
                    var typeName = item.Components[j].GetType().Name;

                    var targetName = GetVarNameByDict(typeName + item.Target.name, j, item);

                    switch (typeName)
                    {
                        case "Transform":
                            codeStr += "\t\t" + targetName + " = viewItemList[" + i + "].Target;\n";
                            break;
                        case "GameObject":
                            codeStr += "\t\t" + targetName + " = viewItemList[" + i + "].Target.gameObject;\n";
                            break;
                        default:
                            codeStr += "\t\t" + targetName + " = viewItemList[" + i + "].Target.GetComponent<"
                                + typeName + ">();\n";
                            break;
                    }
                }
            }
        }

        codeStr += "\t}\n}";
        try
        {
            StreamWriter sw;
            sw = new StreamWriter(new FileStream(realDiskCSFilePath, FileMode.Create, FileAccess.ReadWrite));

            sw.Write(codeStr);
            sw.Close();
            sw.Dispose();
            //Debug.Log(realDiskCSFilePath + " Save Succeed");

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        var comps = newPrefab.GetComponents<YViewReference>();

        if (comps.Length > 1)
        {
            for (int i = 1; i < comps.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(comps[i]);
            }
        }

        if (!IsRunningAll)
        {
            AssetDatabase.Refresh();
        }
    }

    public void GenControlCode()
    {
        GameObject prefab = m_CurItemRef.gameObject;
        string realDiskCSFilePath = ControlCodePath + "/" + Path.GetFileNameWithoutExtension(prefab.name) + "Control.cs";
        List<string> funcNameList = new List<string>();
        string codeStr = "//This File Is Auto Generated By Process Control\n";
        codeStr += "using UnityEngine;\n\npublic class " + Path.GetFileNameWithoutExtension(prefab.name) + "Control : YViewControl\n{\n";


        codeStr += "\tprivate " + Path.GetFileNameWithoutExtension(prefab.name) + "View m_View;\n\n";

        codeStr += "\tpublic static EResType GetResType()\n\t{\n\t\treturn EResType." + Path.GetFileNameWithoutExtension(prefab.name) + ";\n\t}\n\n";

        codeStr += "\tprotected override void OnInit()\n\t{\n\t\tbase.OnInit();\n\t\tm_View = CreateView<" +
             Path.GetFileNameWithoutExtension(prefab.name) + "View>();\n";

        for (int i = 0; i < m_CurItemRef.ViewItemList.Count; i++)
        {
            var item = m_CurItemRef.ViewItemList[i];
            for (int j = 0; j < item.Components.Length; j++)
            {
                var typeName = item.Components[j].GetType().Name;

                var targetName = GetVarNameByDict(typeName + item.Target.name, j, item);

                switch (typeName)
                {
                    case "Button":
                        var funcName = string.Format("On{0}Click", targetName);
                        funcNameList.Add(funcName);
                        codeStr += "\t\tm_View." + targetName + ".onClick.AddListener(On" + targetName + "Click);\n";
                        break;
                    case "TextMeshProUGUI":

                        codeStr += "\t\tm_View." + targetName + ".text = \"\";\n";
                        break;
                }
            }
        }

        codeStr += "\t}\n\n";

        if (funcNameList.Count > 0)
        {
            foreach (var name in funcNameList)
            {
                codeStr += "\tvoid " + name + "()\n\t{\n\t}\n\n";
            }
        }


        codeStr += "\tpublic void SetData()\n\t{\n\t\t;\n\t}\n\n";

        codeStr += "\tprotected override void OnReturn()\n\t{\n\t\tbase.OnReturn();\n\t}\n}";

        try
        {
            StreamWriter sw;
            sw = new StreamWriter(new FileStream(realDiskCSFilePath, FileMode.Create, FileAccess.ReadWrite));

            sw.Write(codeStr);
            sw.Close();
            sw.Dispose();
            Debug.Log(realDiskCSFilePath + " Save Succeed");
            var exPath = Application.dataPath + "/Scripts/Model/EResType.cs";
            var lines = File.ReadAllLines(exPath);
            List<string> list = new List<string>();
            bool isAdded = false;
            var className = Path.GetFileNameWithoutExtension(prefab.name);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(className))
                {
                    isAdded = true;
                    break;
                }
                if (lines[i].Contains("UIUpper"))
                {
                    break;
                }
            }
            if (!isAdded)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    list.Add(lines[i]);
                    if (!isAdded && lines[i + 1].Contains("UIUpper"))
                    {
                        isAdded = true;
                        list.Add("\t" + className + ",");
                        Debug.Log("Add EResType Succed");
                    }
                }
                File.WriteAllLines(exPath, list.ToArray());
            }

            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

}
