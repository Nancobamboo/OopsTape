using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class YActionSystem : YSingletonModule<YActionSystem>
{
    Dictionary<int, Action<object[]>> actions;
    int[] actionNum;

    public void RegistAction(EActionId actionId, Action<object[]> action)
    {
        Action<object[]> value = null;
        if (actions.TryGetValue((int)actionId, out value))
        {
            actions[(int)actionId] += action;
        }
        else
        {
            actions.Add((int)actionId, action);
        }
        actionNum[(int)actionId]++;
    }

    public void UnRegistAction(EActionId actionId, Action<object[]> action)
    {
        Action<object[]> value = null;
        if (actions.TryGetValue((int)actionId, out value))
        {
            actions[(int)actionId] -= action;
            actionNum[(int)actionId]--;
            if (actionNum[(int)actionId] == 0)
            {
                actions.Remove((int)actionId);
            }
        }
    }

    public void UnRegistAction(int actionId, Action<object[]> action)
    {
        Action<object[]> value = null;
        if (actions.TryGetValue(actionId, out value))
        {
            actions[(int)actionId] -= action;
            actionNum[(int)actionId]--;
            if (actionNum[(int)actionId] == 0)
            {
                actions.Remove((int)actionId);
            }
        }
    }

    public void DispatchAction(EActionId actionId, params object[] paraArray)
    {
        Debug.Log("DispatchAction " + actionId.ToString());
        if (actions.ContainsKey((int)actionId))
        {
            actions[(int)actionId]?.Invoke(paraArray);
        }
    }

    protected override void OnInit()
    {
        actions = new Dictionary<int, Action<object[]>>();
        actionNum = new int[(int)EActionId.Upper];
    }

    //protected override void OnClear()
    //{
    //    actions.Clear();
    //    actionNum = null;
    //}
}

public class YItemAction
{
    public int ActionId;
    public Action<object[]> YAction;

    public YItemAction(int actionId, Action<object[]> yAction)
    {
        ActionId = actionId;
        YAction = yAction;
    }
}

public partial class YViewControl
{
    List<YItemAction> m_ActionList = null;

    public void RegistAction(EActionId actionId, Action<object[]> call)
    {
        if (m_ActionList == null)
        {
            m_ActionList = new List<YItemAction>();
        }

        m_ActionList.Add(new YItemAction((int)actionId, call));
        YActionSystem.Instance.RegistAction(actionId, call);
    }

    void UnregistAllAction()
    {
        if (m_ActionList != null)
        {
            foreach (var item in m_ActionList)
            {
                YActionSystem.Instance.UnRegistAction(item.ActionId, item.YAction);
            }

            m_ActionList = null;
        }
    }
}
