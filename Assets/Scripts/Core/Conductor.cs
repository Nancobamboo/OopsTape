using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Conductor : MonoBehaviour
{
    public Chart chart;
    public double songStartDspTime;
    public float secondsPerBeat { get; private set; }

    AudioSource _audio;

    public Action<int> OnBeat;                // 每到整拍广播：0,1,2...
    public Action<double> OnSongTimeSec;      // 每帧广播歌曲秒数
    public Action<double> OnSongBeat;         // 每帧广播歌曲拍数（可小数）

    int _lastWholeBeat = -1;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (chart == null || chart.music == null)
        {
            Debug.LogError("Conductor: Chart或音频未设置");
            enabled = false; return;
        }

        secondsPerBeat = 60f / chart.bpm;
        _audio.clip = chart.music;
        _audio.playOnAwake = false;

        // 用DSP时间启动，避免帧抖动
        songStartDspTime = AudioSettings.dspTime + 0.1f;
        _audio.PlayScheduled(songStartDspTime);
    }

    void Update()
    {
        double dspNow = AudioSettings.dspTime;
        double songTime = Math.Max(0, dspNow - songStartDspTime); // 已播放秒数
        double songBeat = songTime / secondsPerBeat;

        OnSongTimeSec?.Invoke(songTime);
        OnSongBeat?.Invoke(songBeat);

        int wholeBeat = Mathf.FloorToInt((float)songBeat);
        if (wholeBeat != _lastWholeBeat)
        {
            _lastWholeBeat = wholeBeat;
            OnBeat?.Invoke(wholeBeat);
        }
    }

    /// 最近的关键拍距离当前时间（毫秒）
    public double MsToNearestCritical(double beatNow)
    {
        if (chart.criticalBeats == null || chart.criticalBeats.Count == 0) return double.MaxValue;

        double nearest = double.MaxValue;
        foreach (var cb in chart.criticalBeats)
            nearest = Math.Min(nearest, Math.Abs(cb - beatNow));

        double sec = nearest * secondsPerBeat;
        return (sec * 1000.0);
    }

    /// 是否在判定窗口内（ms）
    public bool InHitWindow(double beatNow, float windowMs)
    {
        double ms = MsToNearestCritical(beatNow);
        // 全局偏移（玩家校准）正值表示需要更早按 -> 这里直接把窗口向外拓展
        ms = Math.Abs(ms - chart.globalOffsetMs);
        return ms <= windowMs;
    }

    /// 当前歌曲拍（double）
    public double CurrentBeat()
    {
        double dspNow = AudioSettings.dspTime;
        double songTime = Math.Max(0, dspNow - songStartDspTime);
        return songTime / secondsPerBeat;
    }
}
