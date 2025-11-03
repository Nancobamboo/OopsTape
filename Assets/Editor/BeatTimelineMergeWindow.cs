using System.IO;
using UnityEditor;
using UnityEngine;

public class BeatTimelineMergeWindow : EditorWindow
{
    TextAsset m_JsonA;
    TextAsset m_JsonB;
    string m_StatusMessage = "";
    MessageType m_StatusType = MessageType.None;

    [MenuItem("Tools/Rhythm/Beat Timeline Merger")]
    public static void Open()
    {
        GetWindow<BeatTimelineMergeWindow>("Beat Timeline Merger");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Beat Timeline Merger", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Drag two JSON files (BeatTimelineJson) here:");
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("File A (Target - will be modified):", EditorStyles.boldLabel);
        m_JsonA = (TextAsset)EditorGUILayout.ObjectField(m_JsonA, typeof(TextAsset), false);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("File B (Source - BeatUnits will be copied):", EditorStyles.boldLabel);
        m_JsonB = (TextAsset)EditorGUILayout.ObjectField(m_JsonB, typeof(TextAsset), false);

        EditorGUILayout.Space();

        // Status message
        if (!string.IsNullOrEmpty(m_StatusMessage))
        {
            EditorGUILayout.HelpBox(m_StatusMessage, m_StatusType);
        }

        EditorGUILayout.Space();

        // Merge button
        EditorGUI.BeginDisabledGroup(m_JsonA == null || m_JsonB == null);
        if (GUILayout.Button("Copy B's BeatUnits to A", GUILayout.Height(30)))
        {
            MergeBeatUnits();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will copy all BeatUnits from File B to File A and save File A.\n" +
            "Warning: This operation will overwrite File A's BeatUnits!",
            MessageType.Info);
    }

    void MergeBeatUnits()
    {
        m_StatusMessage = "";
        m_StatusType = MessageType.None;

        try
        {
            // Load JSON files
            string jsonA = m_JsonA.text;
            string jsonB = m_JsonB.text;

            BeatTimelineJson dataA = JsonUtility.FromJson<BeatTimelineJson>(jsonA);
            BeatTimelineJson dataB = JsonUtility.FromJson<BeatTimelineJson>(jsonB);

            if (dataA == null)
            {
                m_StatusMessage = "Failed to parse File A as BeatTimelineJson";
                m_StatusType = MessageType.Error;
                Repaint();
                return;
            }

            if (dataB == null)
            {
                m_StatusMessage = "Failed to parse File B as BeatTimelineJson";
                m_StatusType = MessageType.Error;
                Repaint();
                return;
            }

            // Copy BeatUnits from B to A
            if (dataB.BeatUnits == null)
            {
                m_StatusMessage = "File B has no BeatUnits to copy";
                m_StatusType = MessageType.Warning;
                Repaint();
                return;
            }

            // Create a new list with B's BeatUnits
            dataA.BeatUnits = new System.Collections.Generic.List<BeatUnit>(dataB.BeatUnits);

            // Get the path of File A
            string assetPathA = AssetDatabase.GetAssetPath(m_JsonA);
            if (string.IsNullOrEmpty(assetPathA))
            {
                m_StatusMessage = "Could not find asset path for File A";
                m_StatusType = MessageType.Error;
                Repaint();
                return;
            }

            // Convert to full system path
            string fullPathA = Path.Combine(Application.dataPath, "..", assetPathA).Replace('\\', '/');
            fullPathA = Path.GetFullPath(fullPathA);

            // Save JSON
            string jsonOutput = JsonUtility.ToJson(dataA, true);
            File.WriteAllText(fullPathA, jsonOutput, System.Text.Encoding.UTF8);

            // Refresh asset database
            AssetDatabase.Refresh();

            m_StatusMessage = $"Success! Copied {dataB.BeatUnits.Count} BeatUnits from B to A.\nSaved to: {assetPathA}";
            m_StatusType = MessageType.Info;

            Debug.Log($"BeatTimelineMerge: Copied {dataB.BeatUnits.Count} BeatUnits from '{m_JsonB.name}' to '{m_JsonA.name}'");
        }
        catch (System.Exception e)
        {
            m_StatusMessage = $"Error: {e.Message}\n{e.StackTrace}";
            m_StatusType = MessageType.Error;
            Debug.LogError($"BeatTimelineMerge Error: {e}");
        }

        Repaint();
    }
}

