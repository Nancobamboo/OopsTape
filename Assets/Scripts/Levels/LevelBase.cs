using UnityEngine;

public abstract class LevelBase : MonoBehaviour
{
    public Conductor conductor;
    public BeatJudge judge;

    [Header("开场倒计时拍数")]
    public int introBeats = 4;

    protected bool started;

    protected virtual void Start()
    {
        started = false;
        conductor.OnBeat += HandleBeat;
    }

    void HandleBeat(int wholeBeat)
    {
        if (!started && wholeBeat >= introBeats)
        {
            started = true;
            OnLevelStart();
        }
        OnBeat(wholeBeat);
    }

    protected abstract void OnLevelStart();
    protected virtual void OnBeat(int wholeBeat) { }

    protected virtual void OnDestroy()
    {
        if (conductor != null) conductor.OnBeat -= HandleBeat;
    }
}
