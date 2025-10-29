using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YBaseView
{
    public virtual void OnInit(Transform holder)
    {

    }
}

public partial class YViewControl : MonoBehaviour
{
	public bool IsDirty;
	[HideInInspector]
	AssetSystem m_Asset;

    public AssetSystem Asset
	{
		get {
			if (m_Asset == null)
			{
				Debug.LogWarning("Get Asset By Util");
                m_Asset = ControlUtil.GetAsset();
            }
			return m_Asset;
        }
		set { m_Asset = value; }
	}

	protected T CreateView<T>() where T : YBaseView, new()
	{
		YBaseView view = new T();
		view.OnInit(transform);
		return (T)view;
	}

	private void Awake()
	{
		this.OnInit();
	}

	protected virtual void OnInit()
    {

    }

    public virtual void OnLoadAsset()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
        var textArray = GetComponentsInChildren<Text>(true);
        foreach (var text in textArray)
        {
            text.font = Asset.GameDefaultFont;
        }
#endif
    }

    protected virtual void OnClose()
	{

    }

    protected virtual void OnReturn()
	{
	}


	internal void Close()
    {
        OnClose();
        UnregistAllAction();
        Destroy(gameObject);
	}

    public void Return()
	{
		this.OnReturn();
    }

}
