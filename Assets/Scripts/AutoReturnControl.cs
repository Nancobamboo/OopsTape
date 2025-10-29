using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoReturnControl : YViewControl
{
    public void SetData(float delayTime)
    {
        Invoke("Return", delayTime);
    }

    protected override void OnInit()
    {
        base.OnInit();
    }

    protected override void OnReturn()
    {
        CancelInvoke();

        base.OnReturn();
        if (Asset != null)
        {
            Asset.ReturnPoolItem(this);
        }
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }

    public void TryReturn(params object[] data)
    {
        if (gameObject.activeInHierarchy)
        {
            Return();
        }
    }
}
