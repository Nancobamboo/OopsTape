using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rhythm/Chart")]
public class Chart : ScriptableObject
{
    [Header("BPM 与对齐")]
    public AudioClip music;
    public float bpm = 120f;
    [Tooltip("歌曲开始对齐的起始小节(可为0)，通常用于预留倒数拍")]
    public int startBeat = 0;
    [Tooltip("全局延迟(毫秒)。用于校准设备/声卡延迟，正值=玩家要更早按。")]
    public float globalOffsetMs = 0f;

    [Header("关键判定拍（单位：拍）")]
    public List<double> criticalBeats = new(); // 例如 [4, 8, 12, 16]
}
