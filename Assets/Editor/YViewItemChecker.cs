using UnityEditor;
using UnityEngine;


[InitializeOnLoad]
public class YViewItemChecker 
{
    static int OFFSET = -16;
    static YViewReference LastItemRef;
    static bool IsSkipError = false;

    static YViewItemChecker()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }

    static bool CheckHasError(GameObject obj)
    {
        if(IsSkipError)
        {
            return false;
        }

        var itemRef = obj.GetComponent<YViewReference>();
        if(itemRef != null)
        {
            if(LastItemRef != itemRef)
            {
                LastItemRef = itemRef;
            }

            if(itemRef.ViewItemList != null && itemRef.ViewItemList.Count > 0)
            {
                foreach(var item in itemRef.ViewItemList)
                {
                    if(item.Target == null)
                    {
                        Debug.LogError("View Ref has error! Root Name:" + itemRef.name);
                        return true;
                    }
                    else if (item.Components != null && item.Components.Length > 0)
                    {
                        foreach (var comp in item.Components)
                        {
                            if (comp == null)
                            {
                                Debug.LogError("View Ref has error! Gameobject Name:" + item.Target.name);
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        if (EditorApplication.isPlaying || EditorApplication.isPaused)
        {
            return;
        }

        var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if(obj == null)
        {
            return;
        }

        bool hasError = CheckHasError(obj);

        if (hasError)
        {
            Undo.RecordObject(obj, obj.name);
            GUI.Label(selectionRect, obj.name, GetErrorGUIStyle());
            var ErrorMsg = "删除步骤错误\n\n" + "请先在ProcessUI Editor中\n   1.Remove This Node\n   2.Export View"; 
            if (EditorUtility.DisplayDialog("Error Prefab", ErrorMsg, "取消", "继续"))
            {
                //Undo.RegisterFullObjectHierarchyUndo(obj, obj.name);
                Undo.RevertAllInCurrentGroup();
            }
            else
            {
                //Undo.RegisterFullObjectHierarchyUndo(obj, obj.name);
                Undo.RevertAllInCurrentGroup();
            }

            Debug.LogError(ErrorMsg);
        }

        GameObject root = null;
        YViewReference itemRef = null;
        if (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewSceneObject(obj))
        {
            itemRef = obj.GetComponentInParent<YViewReference>();
        }
        else
        {
            root = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            if (root != null)
            {
                itemRef = root.GetComponent<YViewReference>();
            }
        }


        if (itemRef != null)
        {
            if(itemRef.IsExistItem(obj.transform))
            {
                if (hasError)
                {
                    GUI.Label(selectionRect, obj.name, GetErrorGUIStyle());
                }
                else if (obj.activeInHierarchy)
                {
                    GUI.Label(selectionRect, obj.name, GetHighLightStyle(obj == root));
                }
                else
                {
                    GUI.Label(selectionRect, obj.name, GetHideHighLightStyle(obj == root));
                }
            }
        }
    }


    static GUIStyle GetHideHighLightStyle(bool isRoot)
    {
        return new GUIStyle()
        {
            padding =
            {
                left = isRoot ? EditorStyles.label.padding.left - OFFSET :EditorStyles.label.padding.left - OFFSET + 1 ,
                top = EditorStyles.label.padding.top + 1
            },
            normal =
            {
                textColor = new Color32(243,145,169,255)
            },
        };
    }

    static GUIStyle GetHighLightStyle(bool isRoot)
    {
        return new GUIStyle()
        {
            padding =
            {
                left = isRoot? EditorStyles.label.padding.left - OFFSET : EditorStyles.label.padding.left - OFFSET + 1,
                top = EditorStyles.label.padding.top
            },
            normal =
            {
                textColor = Color.cyan
            },
        };
    }

    static GUIStyle GetErrorGUIStyle()
    {
        return new GUIStyle()
        {
            padding =
            {
                left =EditorStyles.label.padding.left - OFFSET,
                top = EditorStyles.label.padding.top
            },
            normal =
            {
                textColor = Color.red
            },
        };
    }
}
