using System.IO;
using UnityEditor;
using UnityEngine;

public class BeatAnalyzerWindow : EditorWindow
{
    AudioClip m_Clip;
    BeatUtility.Settings m_Settings = BeatUtility.Settings.Default;
    BeatUtility.BeatTimeline? m_Timeline;
    Vector2 m_Scroll;

    [MenuItem("Tools/Rhythm/Beat Analyzer")]
    public static void Open()
    {
        GetWindow<BeatAnalyzerWindow>("Beat Analyzer");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("AudioClip", EditorStyles.boldLabel);
        m_Clip = (AudioClip)EditorGUILayout.ObjectField(m_Clip, typeof(AudioClip), false);

        GUILayout.Space(6);
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        m_Settings.BpmMin = EditorGUILayout.FloatField("BPM Min", m_Settings.BpmMin);
        m_Settings.BpmMax = EditorGUILayout.FloatField("BPM Max", m_Settings.BpmMax);

        m_Settings.WindowSize = EditorGUILayout.IntField("Window Size", m_Settings.WindowSize);
        m_Settings.HopSize = EditorGUILayout.IntField("Hop Size", m_Settings.HopSize);
        m_Settings.PhaseSearchSteps = EditorGUILayout.IntSlider("Phase Steps", m_Settings.PhaseSearchSteps, 16, 400);
        m_Settings.SmoothSize = EditorGUILayout.IntSlider("Smooth Size", m_Settings.SmoothSize, 0, 10);

        GUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = m_Clip != null;
        if (GUILayout.Button("Analyze", GUILayout.Height(28)))
        {
            BeatUtility.BeatTimeline t = BeatUtility.AnalyzeBeats(m_Clip, m_Settings);
            m_Timeline = t;
            BeatTimelineJson dto = BeatUtility.BuildJson(t, m_Clip != null ? m_Clip.name : string.Empty);
            BeatLevelWindow.Open(dto, m_Clip);
        }
        GUI.enabled = m_Clip != null;
        if (GUILayout.Button("Load JSON File", GUILayout.Height(28)))
        {
            string audioName = m_Clip != null ? m_Clip.name : string.Empty;
            if (!string.IsNullOrEmpty(audioName))
            {
                string resourcePath = "BeatExports/" + audioName + "_timeline";
                TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
                if (textAsset != null)
                {
                    string json = textAsset.text;
                    BeatTimelineJson dto = JsonUtility.FromJson<BeatTimelineJson>(json);
                    BeatLevelWindow.Open(dto, m_Clip);
                }
                else
                {
                    Debug.LogWarning("Beat json not found: " + resourcePath);
                }
            }
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (m_Timeline.HasValue)
        {
            BeatUtility.BeatTimeline t = m_Timeline.Value;
            EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("BPM", t.Bpm.ToString("F2"));
            EditorGUILayout.LabelField("Seconds/Beat", t.SecondsPerBeat.ToString("F4"));
            EditorGUILayout.LabelField("Offset (s)", t.OffsetSeconds.ToString("F4"));
            EditorGUILayout.LabelField("Length (s)", t.LengthSeconds.ToString("F2"));

            GUILayout.Space(6);
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            for (int i = 0; i < t.BeatTimes.Count; i++)
            {
                EditorGUILayout.LabelField($"Beat {i}", t.BeatTimes[i].ToString("F4"));
            }
            EditorGUILayout.EndScrollView();
        }
    }
}


