using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class BeatUnitEditor : EditorWindow
{
    static BeatUnitEditor s_Instance;
    BeatUnit m_Unit;
    BeatLevelWindow m_Owner;
    List<GameObject> m_SceneObjects = new List<GameObject>();
    List<string> m_Anims = new List<string>();
    int m_SelectedIndex = -1;
    Dictionary<string, GameObject> m_QuickDict = new Dictionary<string, GameObject>();
    Dictionary<string, string> m_QuickAnimDict = new Dictionary<string, string>();
    Vector2 m_Scroll;
    List<BeatUnit> m_UnitTemplates = new List<BeatUnit>();
    Color[] m_TemplateColors = new Color[]
    {
        new Color(0.8f, 0.4f, 0.4f),  // 红色
        new Color(0.4f, 0.8f, 0.4f),  // 绿色
        new Color(0.4f, 0.4f, 0.8f),  // 蓝色
        new Color(0.8f, 0.8f, 0.4f),  // 黄色
        new Color(0.8f, 0.4f, 0.8f),  // 紫色
        new Color(0.4f, 0.8f, 0.8f),  // 青色
        new Color(0.8f, 0.6f, 0.4f),  // 橙色
        new Color(0.6f, 0.4f, 0.8f),  // 紫罗兰色
    };

    public static void Open(BeatLevelWindow owner, BeatUnit unit)
    {
        BeatUnitEditor w = GetWindow<BeatUnitEditor>(true, "Beat Unit");
        s_Instance = w;
        w.m_Owner = owner;
        w.m_Unit = unit;
        w.minSize = new Vector2(680, 420);
        w.ReloadFromUnit();
        w.Show();
        w.Focus();
    }

    void ReloadFromUnit()
    {
        m_SceneObjects.Clear();
        m_Anims.Clear();
        m_QuickDict.Clear();
        m_QuickAnimDict.Clear();
        m_SelectedIndex = -1;
        if (m_Unit != null)
        {
            if (m_Unit.SceneObjects != null)
            {
                for (int i = 0; i < m_Unit.SceneObjects.Count; i++)
                {
                    GameObject go = FindSceneObjectByName(m_Unit.SceneObjects[i]);
                    m_SceneObjects.Add(go);
                }
            }
            if (m_Unit.AnimList != null)
            {
                for (int i = 0; i < m_Unit.AnimList.Count; i++) m_Anims.Add(m_Unit.AnimList[i]);
            }
        }
        BuildQuickTemplatesFromScene();
        NormalizeCounts();
    }

    void BuildQuickTemplatesFromScene()
    {
        m_QuickDict.Clear();
        m_QuickAnimDict.Clear();
        Animator[] animators = Resources.FindObjectsOfTypeAll<Animator>();
        for (int i = 0; i < animators.Length; i++)
        {
            Animator a = animators[i];
            if (a == null) continue;
            GameObject go = a.gameObject;
            if (!go.scene.IsValid()) continue;
            // Populate dictionaries inside GatherAnimNames
            GatherAnimNames(go);
        }
    }

    void NormalizeCounts()
    {
        while (m_Anims.Count < m_SceneObjects.Count) m_Anims.Add(string.Empty);
        if (m_Anims.Count > m_SceneObjects.Count) m_Anims.RemoveRange(m_SceneObjects.Count, m_Anims.Count - m_SceneObjects.Count);
    }

    GameObject FindSceneObjectByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        return GameObject.Find(name);
    }

    List<string> GatherAnimNames(GameObject go)
    {
        List<string> list = new List<string>();
        if (go == null) return list;
        Animator animator = go.GetComponent<Animator>();
        if (animator == null) return list;
        RuntimeAnimatorController rac = animator.runtimeAnimatorController;
        AnimatorController ac = rac as AnimatorController;
        if (ac == null) return list;
        for (int l = 0; l < ac.layers.Length; l++)
        {
            AnimatorStateMachine sm = ac.layers[l].stateMachine;
            ChildAnimatorState[] states = sm.states;
            for (int s = 0; s < states.Length; s++)
            {
                if (states[s].state != null)
                {
                    string stateName = states[s].state.name;
                    string key = go.name + "_" + stateName;
                    list.Add(key);
                    m_QuickDict[key] = go;
                    m_QuickAnimDict[key] = stateName;
                }
            }
        }
        return list;
    }

    void ApplyAndSave()
    {
        SyncUIToUnit();
        if (m_Owner != null)
        {
            m_Owner.UpdateUnit(m_Unit);
            m_Owner.SaveJson();
        }
    }

    void SyncUIToUnit()
    {
        if (m_Unit == null) return;
        if (m_Unit.SceneObjects == null)
        {
            m_Unit.SceneObjects = new List<string>();
        }
        if (m_Unit.AnimList == null)
        {
            m_Unit.AnimList = new List<string>();
        }
        m_Unit.SceneObjects.Clear();
        m_Unit.AnimList.Clear();
        for (int i = 0; i < m_SceneObjects.Count; i++)
        {
            GameObject go = m_SceneObjects[i];
            if (go != null)
            {
                m_Unit.SceneObjects.Add(go.name);
            }
            else
            {
                m_Unit.SceneObjects.Add(string.Empty);
            }
            string anim = i < m_Anims.Count ? (m_Anims[i] ?? string.Empty) : string.Empty;
            m_Unit.AnimList.Add(anim);
        }
    }

    void QuickSaveUnitTemplate()
    {
        if (m_Unit == null) return;
        // 先同步UI数据到m_Unit
        SyncUIToUnit();
        // 深拷贝当前的BeatUnit作为模板
        BeatUnit template = new BeatUnit
        {
            BeatId = m_Unit.BeatId,
            IsHit = m_Unit.IsHit,
            SceneObjects = new List<string>(),
            AnimList = new List<string>()
        };
        if (m_Unit.SceneObjects != null)
        {
            foreach (string obj in m_Unit.SceneObjects)
            {
                template.SceneObjects.Add(obj);
            }
        }
        if (m_Unit.AnimList != null)
        {
            foreach (string anim in m_Unit.AnimList)
            {
                template.AnimList.Add(anim);
            }
        }
        m_UnitTemplates.Add(template);
        Repaint(); // 刷新界面以显示新添加的模板
    }

    void ApplyTemplateToCurrentUnit(BeatUnit template)
    {
        if (m_Unit == null || template == null) return;
        // 应用模板的除BeatId外的所有值到当前unit
        m_Unit.IsHit = template.IsHit;

        // 复制SceneObjects和AnimList
        if (template.SceneObjects != null)
        {
            if (m_Unit.SceneObjects == null) m_Unit.SceneObjects = new List<string>();
            m_Unit.SceneObjects.Clear();
            foreach (string obj in template.SceneObjects)
            {
                m_Unit.SceneObjects.Add(obj);
            }
        }
        else
        {
            if (m_Unit.SceneObjects == null) m_Unit.SceneObjects = new List<string>();
            m_Unit.SceneObjects.Clear();
        }

        if (template.AnimList != null)
        {
            if (m_Unit.AnimList == null) m_Unit.AnimList = new List<string>();
            m_Unit.AnimList.Clear();
            foreach (string anim in template.AnimList)
            {
                m_Unit.AnimList.Add(anim);
            }
        }
        else
        {
            if (m_Unit.AnimList == null) m_Unit.AnimList = new List<string>();
            m_Unit.AnimList.Clear();
        }

        // 重新加载UI显示
        ReloadFromUnit();
        // 保存更改
        ApplyAndSave();
    }

    void OnDestroy()
    {
        if (s_Instance == this) s_Instance = null;
    }

    void OnGUI()
    {
        if (m_Unit == null)
        {
            EditorGUILayout.LabelField("No Unit");
            return;
        }

        EditorGUILayout.LabelField("Beat Id", m_Unit.BeatId.ToString());
        EditorGUI.BeginChangeCheck();
        bool newIsHit = EditorGUILayout.Toggle("Is Hit", m_Unit.IsHit);
        if (EditorGUI.EndChangeCheck())
        {
            m_Unit.IsHit = newIsHit;
            ApplyAndSave();
        }

        m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
        int removeIndex = -1;
        for (int i = 0; i < m_SceneObjects.Count; i++)
        {
            int row = i;
            EditorGUILayout.BeginHorizontal();
            Animator currentAnim = m_SceneObjects[row] != null ? m_SceneObjects[row].GetComponent<Animator>() : null;
            EditorGUI.BeginChangeCheck();
            Animator newAnim = (Animator)EditorGUILayout.ObjectField(currentAnim, typeof(Animator), true);
            if (EditorGUI.EndChangeCheck())
            {
                GameObject newGo = newAnim != null ? newAnim.gameObject : null;
                m_SceneObjects[row] = newGo;
                m_SelectedIndex = row;
                if (newGo != null)
                {
                    if (m_Unit.SceneObjects == null) m_Unit.SceneObjects = new List<string>();
                    while (m_Unit.SceneObjects.Count <= row) m_Unit.SceneObjects.Add(string.Empty);
                    m_Unit.SceneObjects[row] = newGo.name;
                    // removed m_AnimCandidates usage
                }
            }
            EditorGUI.BeginChangeCheck();
            string newAnimTxt = EditorGUILayout.TextField(m_Anims[row]);
            if (EditorGUI.EndChangeCheck())
            {
                m_Anims[row] = newAnimTxt;
                ApplyAndSave();
            }
            if (GUILayout.Button("X", GUILayout.Width(24))) removeIndex = row;
            EditorGUILayout.EndHorizontal();
        }
        if (removeIndex >= 0)
        {
            m_SceneObjects.RemoveAt(removeIndex);
            m_Anims.RemoveAt(removeIndex);
            if (m_SelectedIndex == removeIndex) { m_SelectedIndex = -1; }
            ApplyAndSave();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.LabelField("Quick Templates");
        var quickList = new List<KeyValuePair<string, GameObject>>(m_QuickDict);
        foreach (KeyValuePair<string, GameObject> kv in quickList)
        {
            string disp = kv.Key;
            if (GUILayout.Button(disp, GUI.skin.button))
            {
                int newIndex = m_SceneObjects.Count;
                m_SceneObjects.Add(kv.Value);
                string animName;
                if (!m_QuickAnimDict.TryGetValue(kv.Key, out animName)) animName = kv.Key;
                m_Anims.Add(animName);
                m_SelectedIndex = newIndex;
                // removed m_AnimCandidates usage
                ApplyAndSave();
            }
        }

        EditorGUILayout.Space(10); // 添加间距，让按钮往下一点

        EditorGUILayout.BeginHorizontal();
        Color old = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.25f, 0.8f, 0.35f);
        if (GUILayout.Button("QuickSaveUnitTemplate", GUILayout.Height(26)))
        {
            QuickSaveUnitTemplate();
        }
        GUI.backgroundColor = old;
        EditorGUILayout.EndHorizontal();

        // 显示模板列表
        if (m_UnitTemplates.Count > 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Unit Templates", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < m_UnitTemplates.Count; i++)
            {
                BeatUnit template = m_UnitTemplates[i];
                Color templateColor = m_TemplateColors[i % m_TemplateColors.Length];
                Color oldBg = GUI.backgroundColor;
                GUI.backgroundColor = templateColor;
                if (GUILayout.Button(template.BeatId.ToString(), GUILayout.Width(60)))
                {
                    ApplyTemplateToCurrentUnit(template);
                }
                GUI.backgroundColor = oldBg;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}


