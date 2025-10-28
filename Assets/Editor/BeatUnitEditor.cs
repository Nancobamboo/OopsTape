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
    List<string> m_AnimCandidates = new List<string>();
    int m_SelectedIndex = -1;
    Dictionary<string, GameObject> m_QuickDict = new Dictionary<string, GameObject>();
    Vector2 m_Scroll;

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
        m_AnimCandidates.Clear();
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
        NormalizeCounts();
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
                if (states[s].state != null) list.Add(states[s].state.name);
            }
        }
        return list;
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

        m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
        int removeIndex = -1;
        for (int i = 0; i < m_SceneObjects.Count; i++)
        {
            int row = i;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            GameObject newGo = (GameObject)EditorGUILayout.ObjectField(m_SceneObjects[row], typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                m_SceneObjects[row] = newGo;
                m_SelectedIndex = row;
                m_AnimCandidates = GatherAnimNames(newGo);
                if (newGo != null)
                {
                    if (m_Unit.SceneObjects == null) m_Unit.SceneObjects = new List<string>();
                    while (m_Unit.SceneObjects.Count <= row) m_Unit.SceneObjects.Add(string.Empty);
                    m_Unit.SceneObjects[row] = newGo.name;
                }
            }
            m_Anims[row] = EditorGUILayout.TextField(m_Anims[row]);
            if (GUILayout.Button("X", GUILayout.Width(24))) removeIndex = row;
            EditorGUILayout.EndHorizontal();

            if (m_SelectedIndex == row && m_AnimCandidates.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                for (int c = 0; c < m_AnimCandidates.Count; c++)
                {
                    if (GUILayout.Button(m_AnimCandidates[c], GUI.skin.button, GUILayout.Width(160)))
                    {
                        m_Anims[row] = m_AnimCandidates[c];
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        if (removeIndex >= 0)
        {
            m_SceneObjects.RemoveAt(removeIndex);
            m_Anims.RemoveAt(removeIndex);
            if (m_SelectedIndex == removeIndex) { m_SelectedIndex = -1; m_AnimCandidates.Clear(); }
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.LabelField("Quick Templates");
        foreach (KeyValuePair<string, GameObject> kv in m_QuickDict)
        {
            string disp = (kv.Value != null ? kv.Value.name : "null") + " _ " + kv.Key;
            if (GUILayout.Button(disp, GUI.skin.button))
            {
                int newIndex = m_SceneObjects.Count;
                m_SceneObjects.Add(kv.Value);
                m_Anims.Add(kv.Key);
                m_SelectedIndex = newIndex;
                m_AnimCandidates = GatherAnimNames(kv.Value);
            }
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Slot"))
        {
            m_SceneObjects.Add(null);
            m_Anims.Add(string.Empty);
        }
        if (GUILayout.Button("Apply"))
        {
            if (m_Unit.SceneObjects == null) m_Unit.SceneObjects = new List<string>();
            if (m_Unit.AnimList == null) m_Unit.AnimList = new List<string>();
            m_Unit.SceneObjects.Clear();
            m_Unit.AnimList.Clear();
            for (int i = 0; i < m_SceneObjects.Count; i++)
            {
                GameObject go = m_SceneObjects[i];
                string anim = i < m_Anims.Count ? (m_Anims[i] ?? string.Empty) : string.Empty;
                if (go != null) m_Unit.SceneObjects.Add(go.name); else m_Unit.SceneObjects.Add(string.Empty);
                m_Unit.AnimList.Add(anim);
                if (go != null && !string.IsNullOrEmpty(anim)) m_QuickDict[anim] = go;
            }
            if (m_Owner != null)
            {
                m_Owner.UpdateUnit(m_Unit);
                m_Owner.SaveJson();
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}


