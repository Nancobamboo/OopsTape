using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IYSingleton
{
    void Init();
    void Clear();
}

public static class YSingletonContext
{
    static private List<IYSingleton> Modules = new List<IYSingleton>();
    static internal void RegisterModule(IYSingleton module)
    {
        Modules.Add(module);
    }

    static public void InitModules()
    {
        foreach (IYSingleton s in Modules)
        {
            s.Init();
        }
    }

    static public void ClearModules()
    {
        foreach (IYSingleton s in Modules)
        {
            s.Clear();
        }
    }
}

public abstract class YSingletonModule<T> : IYSingleton where T : YSingletonModule<T>, new()
{
    private static T _instance = default(T);
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
                _instance.Init();
            }
            return _instance;
        }
    }
    private bool m_HasInited = false;

    public YSingletonModule()
    {
        YSingletonContext.RegisterModule(this);
    }

    public void Init()
    {
        if (m_HasInited == false)
        {
            OnInit();
            m_HasInited = true;
        }
    }

    protected abstract void OnInit();

    public void Clear()
    {
       
    }
}