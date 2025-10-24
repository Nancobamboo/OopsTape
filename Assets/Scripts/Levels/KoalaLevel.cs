using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KoalaLevel : LevelBase
{
    public GameObject eyesClosed;
    public GameObject eyesOpen;
    public Image flashOverlay;
    public Text judgeText;

    [Header("考拉睁眼时长(秒)")]
    public float eyesOpenDuration = 0.20f;

    [Header("玩家按键")]
    public KeyCode key = KeyCode.Space;

    void Update()
    {
        if (!started) return;

        if (Input.GetKeyDown(key))
        {
            var res = judge.PressNow();
            ShowJudge(res);
            if (res != BeatJudge.Judge.Miss)
                StartCoroutine(Flash());
        }
    }

    protected override void OnLevelStart()
    {
        // 可播放“开始”SFX/做个UI淡入
    }

    protected override void OnBeat(int wholeBeat)
    {
        // 若此整拍接近关键拍，则让考拉“眨眼=睁开一小会儿”
        double nowBeat = conductor.CurrentBeat();
        if (conductor.InHitWindow(nowBeat, 120f)) // 这一帧落在关键拍附近
            StartCoroutine(OpenEyesBrief());
    }

    IEnumerator OpenEyesBrief()
    {
        eyesClosed.SetActive(false);
        eyesOpen.SetActive(true);
        yield return new WaitForSeconds(eyesOpenDuration);
        eyesOpen.SetActive(false);
        eyesClosed.SetActive(true);
    }

    IEnumerator Flash()
    {
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            flashOverlay.color = new Color(1,1,1, Mathf.Lerp(0, 0.8f, t/0.05f));
            yield return null;
        }
        t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            flashOverlay.color = new Color(1,1,1, Mathf.Lerp(0.8f, 0f, t/0.2f));
            yield return null;
        }
        flashOverlay.color = new Color(1,1,1,0);
    }

    void ShowJudge(BeatJudge.Judge j)
    {
        judgeText.text = j.ToString();
        // 可加颜色/动画
    }
}
