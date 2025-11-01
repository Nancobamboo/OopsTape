using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class BeatLevelWindow : EditorWindow
{
    BeatTimelineJson m_Data;
    Vector2 m_Scroll;
    List<string> m_RowSecond;
    AudioClip m_Clip;

    const float kCellWidth = 60;
    float m_SliderSeconds = 0f; // current slider time in seconds
    int m_LastSamplePos = 0;     // last known preview sample position

    static System.Type s_AudioUtilType;
    static MethodInfo s_StopAllPrev;
    static MethodInfo s_PlayPrev3;
    static MethodInfo s_PlayPrev4;
    static MethodInfo s_GetPos;
    static MethodInfo s_SetPos;
    static MethodInfo s_IsPlaying0;
    static MethodInfo s_IsPlaying1;

    double m_EndSample = -1;
    bool m_Watching = false;

    public static void Open(BeatTimelineJson data, AudioClip clip = null)
    {
        BeatLevelWindow w = CreateInstance<BeatLevelWindow>();
        w.titleContent = new GUIContent("Beat Level");
        w.m_Data = data;
        w.m_Clip = clip;
        w.EnsureAudioUtil();
        w.Show();
    }

    void EnsureAudioUtil()
    {
        if (s_AudioUtilType != null) return;
        s_AudioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil")
            ?? System.Type.GetType("UnityEditor.AudioUtil, UnityEditor")
            ?? System.Type.GetType("UnityEditorInternal.AudioUtil, UnityEditor");
        if (s_AudioUtilType == null) return;
        BindingFlags B = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        s_StopAllPrev = s_AudioUtilType.GetMethod("StopAllPreviewClips", B);
        foreach (MethodInfo mi in s_AudioUtilType.GetMethods(B))
        {
            if (mi.Name == "PlayPreviewClip")
            {
                var ps = mi.GetParameters();
                if (ps.Length == 3) s_PlayPrev3 = mi;
                else if (ps.Length == 4) s_PlayPrev4 = mi;
            }
        }
        s_GetPos = s_AudioUtilType.GetMethod("GetPreviewClipSamplePosition", B);
        s_SetPos = s_AudioUtilType.GetMethod("SetPreviewClipSamplePosition", B);
        s_IsPlaying0 = s_AudioUtilType.GetMethod("IsPreviewClipPlaying", B, null, System.Type.EmptyTypes, null);
        s_IsPlaying1 = s_AudioUtilType.GetMethod("IsPreviewClipPlaying", B, null, new System.Type[] { typeof(AudioClip) }, null);
    }

    public void UpdateUnit(BeatUnit unit)
    {
        if (m_Data == null) return;
        if (m_Data.BeatUnits == null) m_Data.BeatUnits = new List<BeatUnit>();
        bool replaced = false;
        for (int i = 0; i < m_Data.BeatUnits.Count; i++)
        {
            if (m_Data.BeatUnits[i].BeatId == unit.BeatId)
            {
                m_Data.BeatUnits[i] = unit;
                replaced = true;
                break;
            }
        }
        if (!replaced) m_Data.BeatUnits.Add(unit);
        Repaint();
    }

    public void SaveJson()
    {
        if (m_Data == null) return;
        if (m_Data.BeatUnits == null) m_Data.BeatUnits = new List<BeatUnit>();
        string folder = Path.Combine(Application.dataPath, "BeatExports");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        string file = (!string.IsNullOrEmpty(m_Data.AudioName) ? m_Data.AudioName : "Beat") + "_timeline.json";
        string path = Path.Combine(folder, file);
        string json = JsonUtility.ToJson(m_Data, true);
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        Debug.Log("Saved beat json: " + path + "  Units=" + m_Data.BeatUnits.Count + "  JsonLen=" + json.Length);
    }

    void InitRowSecond()
    {
        int count = m_Data.BeatTimes != null ? m_Data.BeatTimes.Count : 0;
        if (m_RowSecond == null || m_RowSecond.Count != count)
        {
            m_RowSecond = new List<string>(count);
            for (int i = 0; i < count; i++) m_RowSecond.Add("O");
            if (m_Data.BeatUnits != null)
            {
                for (int u = 0; u < m_Data.BeatUnits.Count; u++)
                {
                    int bid = m_Data.BeatUnits[u].BeatId;
                    if (bid >= 0 && bid < count) m_RowSecond[bid] = "S";
                }
            }
        }
    }

    AudioClip FindClip()
    {
        if (m_Clip != null) return m_Clip;
        if (string.IsNullOrEmpty(m_Data.AudioName)) return null;
        string[] guids = AssetDatabase.FindAssets(m_Data.AudioName + " t:AudioClip");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null && clip.name == m_Data.AudioName)
            {
                m_Clip = clip;
                return clip;
            }
        }
        return null;
    }

    void PlaySegment(AudioClip clip, double startSeconds, double endSeconds)
    {
        EnsureAudioUtil();
        if (s_AudioUtilType == null || clip == null) return;
        int start = Mathf.Clamp((int)(startSeconds * clip.frequency), 0, clip.samples - 1);
        int end = Mathf.Clamp((int)(endSeconds * clip.frequency), 0, clip.samples - 1);
        try
        {
            s_StopAllPrev?.Invoke(null, null);
            if (s_PlayPrev3 != null) s_PlayPrev3.Invoke(null, new object[] { clip, start, false });
            else if (s_PlayPrev4 != null) s_PlayPrev4.Invoke(null, new object[] { clip, start, false, false });
            if (s_SetPos != null) s_SetPos.Invoke(null, new object[] { clip, start });
            m_EndSample = end;
            m_Watching = true;
            EditorApplication.update -= WatchPreview;
            EditorApplication.update += WatchPreview;
        }
        catch { }
    }

    int GetBeatIndexAtTime(double timeSeconds)
    {
        if (m_Data == null) return 0;
        int count = m_Data.BeatTimes != null ? m_Data.BeatTimes.Count : 0;
        if (count <= 0) return 0;
        const double eps = 1e-6; // guard against floating point issues
        for (int i = 0; i < count; i++)
        {
            double end = i < m_Data.BeatTimes.Count ? m_Data.BeatTimes[i] : (m_Data.OffsetSeconds + i * m_Data.SecondsPerBeat);
            if (timeSeconds <= end - eps) return i; // inclusive boundary
        }
        return Mathf.Max(0, count - 1);
    }

    void EnsurePlayheadVisible(float playSeconds)
    {
        AudioClip clip = FindClip();
        if (clip == null || clip.length <= 0f) return;
        int count = m_Data.BeatTimes != null ? m_Data.BeatTimes.Count : 0;
        if (count <= 0) return;
        float sliderWidth = count * kCellWidth;
        float viewportWidth = Mathf.Max(0f, position.width - 32f); // approximate visible width minus scrollbar/margins
        float padding = Mathf.Min(40f, kCellWidth * 0.5f);
        float playX = Mathf.Clamp01(playSeconds / clip.length) * sliderWidth;
        float viewStart = m_Scroll.x;
        float viewEnd = m_Scroll.x + viewportWidth;
        // If near edges, jump-scroll by half of the viewport width
        if (playX > viewEnd - padding)
        {
            float targetStart = viewStart + viewportWidth * 0.95f;
            m_Scroll.x = Mathf.Clamp(targetStart, 0f, Mathf.Max(0f, sliderWidth - viewportWidth));
        }
        else if (playX < viewStart + padding)
        {
            float targetStart = viewStart - viewportWidth * 0.95f;
            m_Scroll.x = Mathf.Clamp(targetStart, 0f, Mathf.Max(0f, sliderWidth - viewportWidth));
        }
    }

    void WatchPreview()
    {
        if (!m_Watching || s_AudioUtilType == null) return;
        bool playing = false;
        try
        {
            if (s_IsPlaying0 != null) playing = (bool)s_IsPlaying0.Invoke(null, null);
            else if (s_IsPlaying1 != null) playing = (bool)s_IsPlaying1.Invoke(null, new object[] { m_Clip });
            if (!playing) { m_Watching = false; EditorApplication.update -= WatchPreview; return; }
            if (s_GetPos != null)
            {
                int pos = (int)s_GetPos.Invoke(null, null);
                m_LastSamplePos = pos;
                AudioClip clip = FindClip();
                if (clip != null && clip.frequency > 0)
                {
                    m_SliderSeconds = Mathf.Clamp01((float)pos / (float)clip.samples) * clip.length;
                    EnsurePlayheadVisible(m_SliderSeconds);
                }
                Repaint();
                if (pos >= m_EndSample)
                {
                    s_StopAllPrev?.Invoke(null, null);
                    m_Watching = false;
                    EditorApplication.update -= WatchPreview;
                }
            }
        }
        catch { m_Watching = false; EditorApplication.update -= WatchPreview; }
    }

    void OnDisable()
    {
        EditorApplication.update -= WatchPreview;
        try { s_StopAllPrev?.Invoke(null, null); } catch { }
        m_Watching = false;
    }

    void OnGUI()
    {
        if (m_Data == null)
        {
            EditorGUILayout.LabelField("No Data");
            return;
        }

        InitRowSecond();

        EditorGUILayout.LabelField("Audio", m_Data.AudioName);
        EditorGUILayout.LabelField("Beats", (m_Data.BeatTimes != null ? m_Data.BeatTimes.Count : 0).ToString());

        // Playback controls row: Play/Pause + progress slider
        AudioClip topClip = FindClip();
        int count = m_Data.BeatTimes != null ? m_Data.BeatTimes.Count : 0;
        float totalSeconds = 0f;
        if (m_Data.BeatTimes != null && m_Data.BeatTimes.Count > 0)
            totalSeconds = m_Data.BeatTimes[m_Data.BeatTimes.Count - 1];
        else if (topClip != null)
            totalSeconds = topClip.length;
        float sliderWidth = m_Data.BeatTimes.Count * kCellWidth;
        bool isPlaying = false;
        try
        {
            if (s_IsPlaying0 != null) isPlaying = (bool)s_IsPlaying0.Invoke(null, null);
            else if (s_IsPlaying1 != null) isPlaying = (bool)s_IsPlaying1.Invoke(null, new object[] { topClip });
        }
        catch { }

        // Row A: Play/Pause button
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(topClip == null);
        if (GUILayout.Button(isPlaying ? "Pause" : "Play", GUILayout.Width(60)))
        {
            if (isPlaying)
            {
                try { s_StopAllPrev?.Invoke(null, null); } catch { }
            }
            else if (topClip != null)
            {
                int startSample = Mathf.Clamp((int)(m_SliderSeconds * topClip.frequency), 0, Mathf.Max(0, topClip.samples - 1));
                try
                {
                    s_StopAllPrev?.Invoke(null, null);
                    if (s_PlayPrev3 != null) s_PlayPrev3.Invoke(null, new object[] { topClip, startSample, false });
                    else if (s_PlayPrev4 != null) s_PlayPrev4.Invoke(null, new object[] { topClip, startSample, false, false });
                    if (s_SetPos != null) s_SetPos.Invoke(null, new object[] { topClip, startSample });
                    m_EndSample = topClip.samples - 1;
                    m_Watching = true;
                    EditorApplication.update -= WatchPreview;
                    EditorApplication.update += WatchPreview;
                }
                catch { }
            }
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
        // Row B: Progress slider (inside scroll view to sync horizontal scroll)
        EditorGUILayout.BeginHorizontal(GUIStyle.none);
        EditorGUI.BeginDisabledGroup(topClip == null);
        Rect sliderRect = GUILayoutUtility.GetRect(sliderWidth, 18);
        float newSlider = GUI.HorizontalSlider(sliderRect,
            topClip != null ? m_SliderSeconds : 0f,
            0f, topClip != null ? topClip.length : 1f);
        if (topClip != null && !Mathf.Approximately(newSlider, m_SliderSeconds))
        {
            m_SliderSeconds = newSlider;
            int newSample = Mathf.Clamp((int)(m_SliderSeconds * topClip.frequency), 0, Mathf.Max(0, topClip.samples - 1));
            try { if (s_SetPos != null) s_SetPos.Invoke(null, new object[] { topClip, newSample }); } catch { }
            EnsurePlayheadVisible(m_SliderSeconds);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(6);
        // Highlight current beat based on slider time
        int highlightBeat = GetBeatIndexAtTime(m_SliderSeconds);

        // Row 1: Index buttons (play segments)
        EditorGUILayout.BeginHorizontal(GUIStyle.none);
        for (int i = 0; i < count; i++)
        {
            EditorGUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Width(kCellWidth));
            Color prev = GUI.backgroundColor;
            if (i == highlightBeat) GUI.backgroundColor = new Color(0.95f, 0.85f, 0.2f);
            if (GUILayout.Button(i.ToString(), GUILayout.ExpandWidth(true)))
            {
                AudioClip clip = FindClip();
                if (clip != null)
                {
                    double end = i < m_Data.BeatTimes.Count ? m_Data.BeatTimes[i] : (m_Data.OffsetSeconds + i * m_Data.SecondsPerBeat);
                    double start = 0.0;
                    if (i > 0)
                        start = (i - 1) < m_Data.BeatTimes.Count ? m_Data.BeatTimes[i - 1] : (m_Data.OffsetSeconds + (i - 1) * m_Data.SecondsPerBeat);
                    PlaySegment(clip, start, end);
                }
            }
            GUI.backgroundColor = prev;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        // Row 2: State field (click to open BeatUnit editor)
        EditorGUILayout.BeginHorizontal(GUIStyle.none);
        for (int i = 0; i < count; i++)
        {
            string second = m_RowSecond[i];
            if (m_Data.BeatUnits != null)
            {
                BeatUnit unitS = m_Data.BeatUnits.Find(u => u.BeatId == i);
                if (unitS != null)
                {
                    if (unitS.IsHit)
                    {
                        second = "Hit";
                    }
                    else
                    {
                        bool emptyAnim = (unitS.AnimList == null || unitS.AnimList.Count == 0);
                        if (emptyAnim) second = "O";
                        else if (unitS.IsHit) second = "Hit";
                    }
                    if (unitS.IsTutor)
                    {
                        second += "+T";
                    }
                }
            }
            EditorGUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Width(kCellWidth));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(second, GUILayout.ExpandWidth(true));
            EditorGUI.EndDisabledGroup();
            Rect secondRect = GUILayoutUtility.GetLastRect();
            EditorGUILayout.EndHorizontal();
            if (GUI.Button(secondRect, GUIContent.none, GUIStyle.none))
            {
                if (m_Data.BeatUnits == null) m_Data.BeatUnits = new List<BeatUnit>();
                BeatUnit unit = m_Data.BeatUnits.Find(u => u.BeatId == i);
                if (unit == null)
                {
                    unit = new BeatUnit { BeatId = i, SceneObjects = new List<string>(), AnimList = new List<string>(), IsHit = false };
                    m_Data.BeatUnits.Add(unit);
                    if (m_RowSecond != null && i >= 0 && i < m_RowSecond.Count) m_RowSecond[i] = "S";
                }
                BeatUnitEditor.Open(this, unit);
            }
        }
        EditorGUILayout.EndHorizontal();

        // Row 3: Beat time fields
        EditorGUILayout.BeginHorizontal(GUIStyle.none);
        for (int i = 0; i < count; i++)
        {
            float bt = (m_Data.BeatTimes != null && i < m_Data.BeatTimes.Count) ? m_Data.BeatTimes[i] : 0f;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Width(kCellWidth));
            float newBt = EditorGUILayout.FloatField(bt, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
            {
                if (m_Data.BeatTimes == null) m_Data.BeatTimes = new List<float>();
                while (m_Data.BeatTimes.Count <= i) m_Data.BeatTimes.Add(0f);
                m_Data.BeatTimes[i] = newBt;
                SaveJson();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        // Row 4: Add/Remove unit buttons
        EditorGUILayout.BeginHorizontal(GUIStyle.none);
        for (int i = 0; i < count; i++)
        {
            EditorGUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Width(kCellWidth));
            if (m_RowSecond[i] == "O")
            {
                if (GUILayout.Button("+", GUILayout.Width(30)))
                {
                    m_RowSecond[i] = "S";
                    if (m_Data.BeatUnits == null) m_Data.BeatUnits = new List<BeatUnit>();
                    BeatUnit unit = new BeatUnit { BeatId = i, SceneObjects = new List<string>(), AnimList = new List<string>(), IsHit = false };
                    m_Data.BeatUnits.Add(unit);
                    SaveJson();
                    BeatUnitEditor.Open(this, unit);
                }
            }
            else
            {
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    m_RowSecond[i] = "O";
                    if (m_Data.BeatUnits != null)
                        m_Data.BeatUnits.RemoveAll(u => u.BeatId == i);
                    SaveJson();
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }
}


