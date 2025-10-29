using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class YViewItem
{
    public Transform Target;
#if UNITY_EDITOR
    public Object[] Components;
    public string[] VarNameArray;
#endif
}


public class YViewReference : MonoBehaviour
{
    public List<YViewItem> ViewItemList;

    public bool IsExistItem(Transform target)
    {
        if (ViewItemList != null && ViewItemList.Count > 0)
        {
            foreach (var item in ViewItemList)
            {
                if (target == item.Target)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public YViewItem GetItem(Transform target)
    {

        if (ViewItemList == null)
        {
            ViewItemList = new List<YViewItem>();
        }

        foreach (var item in ViewItemList)
        {
            if (target == item.Target)
            {
                return item;
            }
        }
        var newItem = new YViewItem();
        newItem.Target = target;
        ViewItemList.Add(newItem);
        return newItem;
    }

    public void RemoveItem(YViewItem item)
    {
        if (ViewItemList != null && ViewItemList.Count > 0)
        {
            ViewItemList.Remove(item);
        }
    }

#if UNITY_EDITOR
    public void ExchangeItemPos(int posA, int posB)
    {
        var tmpNode = ViewItemList[posA];
        ViewItemList[posA] = ViewItemList[posB];
        ViewItemList[posB] = tmpNode;
    }

    public void MoveToTargetPos(int posA, int posB)
    {
        if (posA == posB || posA >= ViewItemList.Count || posB >= ViewItemList.Count)
        {
            return;
        }

        if (posA < posB)
        {
            var tmpNode = ViewItemList[posA];
            for (int i = posA; i < posB; i++)
            {
                ViewItemList[i] = ViewItemList[i + 1];
            }
            ViewItemList[posB] = tmpNode;
        }
        else
        {
            var tmpNode = ViewItemList[posA];

            for (int i = posA; i > posB; i--)
            {
                ViewItemList[i] = ViewItemList[i - 1];
            }
            ViewItemList[posB] = tmpNode;
        }

    }
#endif
}
