using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaBehaviour : MonoBehaviour
{
    private LuaEnv m_LuaEnv = Manager.Lua.LuaEnv;
    protected LuaTable m_ScriptEnv;
    private Action m_LuaAwake;
    private Action m_LuaStart;
    private Action m_LuaUpdate;
    private Action m_LuaOnDestroy;

    public string luaName;

    private void Awake()
    {
        // 为每个脚本设置独立环境，一定程度上防止脚本间全局变量、函数冲突
        m_ScriptEnv = m_LuaEnv.NewTable();
        LuaTable meta = m_LuaEnv.NewTable();
        meta.Set("__index", m_LuaEnv.Global);
        m_ScriptEnv.SetMetaTable(meta);
        meta.Dispose();

        // 绑定关键字self
        m_ScriptEnv.Set("self", this);

        // 在Awake之前为绑定的lua脚本赋值
        m_LuaEnv.DoString(Manager.Lua.GetLuaScript(luaName), luaName, m_ScriptEnv);
        m_ScriptEnv.Get("Awake", out m_LuaAwake);
        m_ScriptEnv.Get("Start", out m_LuaStart);
        m_ScriptEnv.Get("Update", out m_LuaUpdate);

        m_LuaAwake?.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_LuaStart?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        m_LuaUpdate?.Invoke();
    }

    protected virtual void Clear()
    {
        // 释放Lua方法
        m_LuaOnDestroy = null;
        m_LuaAwake = null;
        m_LuaStart = null;
        // 释放脚本环境
        m_ScriptEnv?.Dispose();
        m_ScriptEnv = null;
    }

    private void OnDestroy()
    {
        m_LuaOnDestroy?.Invoke();
        Clear();
    }

    private void OnApplicationQuit()
    {
        Clear();
    }
}
