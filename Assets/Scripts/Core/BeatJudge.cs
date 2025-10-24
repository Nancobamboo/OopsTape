using UnityEngine;

public class BeatJudge : MonoBehaviour
{
    public Conductor conductor;
    [Header("判定窗口(ms)")]
    public float perfectWindow = 70f;
    public float goodWindow = 120f;

    public enum Judge { Miss, Good, Perfect }

    public Judge PressNow()
    {
        double beat = conductor.CurrentBeat();
        double deltaMs = conductor.MsToNearestCritical(beat) - conductor.chart.globalOffsetMs;
        deltaMs = Mathf.Abs((float)deltaMs);

        if (deltaMs <= perfectWindow) return Judge.Perfect;
        if (deltaMs <= goodWindow) return Judge.Good;
        return Judge.Miss;
    }
}
