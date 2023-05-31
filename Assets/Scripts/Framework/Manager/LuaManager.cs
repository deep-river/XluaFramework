using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Windows;
using XLua;

public class LuaManager : MonoBehaviour
{
    // 所有lua文件列表
    public List<string> LuaNames = new List<string>();

    // lua文件内容缓存
    private Dictionary<string, byte[]> m_LuaScripts;

    // Lua虚拟机
    public LuaEnv LuaEnv;

    // Lua文件加载完成回调
    Action InitDone;

    public void Init(Action init)
    {
        InitDone += init;

        // Lua虚拟机初始化
        LuaEnv = new LuaEnv();

        // 加载自定义Loader
        LuaEnv.AddLoader(Loader);

        // m_LuaScripts初始化
        m_LuaScripts = new Dictionary<string, byte[]>();

#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
            EditorLoadLuaScript();
        else
#endif
            LoadLuaScript();
    }

    /// <summary>
    /// Lua虚拟机执行Lua文件
    /// 建议的加载Lua脚本方式是：整个程序就一个DoString("require 'main'")，然后在main.lua加载其它脚本（类似lua脚本的命令行执行：lua main.lua）
    /// </summary>
    /// <param name="name"></param>
    public void StartLua(string name)
    {
        LuaEnv.DoString(string.Format("require '{0}'", name));
    }

    /// <summary>
    /// 自定义Loader
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    byte[] Loader(ref string name)
    {
        return GetLuaScript(name);
    }

    /// <summary>
    /// 从m_LuaScripts中查找指定Lua文件
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public byte[] GetLuaScript(string name)
    {
        // 引用路径转为文件路径
        // 例：require ui.login.register -> ui/login/register.bytes
        name = name.Replace(".", "/");
        string fileName = PathUtil.GetLuaPath(name);

        byte[] luaScript = null;
        if (!m_LuaScripts.TryGetValue(fileName, out luaScript))
            Debug.LogError("Lua script does not exist:" + fileName);
        return luaScript;
    }

    public void AddLuaScript(string assetsName, byte[] luaScript)
    {
        m_LuaScripts[assetsName] = luaScript;
    }

    /// <summary>
    /// 将LuaNames中所有Lua文件内容加入m_LuaScripts中
    /// </summary>
    void LoadLuaScript()
    {
        foreach (string name in LuaNames)
        {
            Manager.Resource.LoadLua(name, (UnityEngine.Object obj) => 
            {
                AddLuaScript(name, (obj as TextAsset).bytes);
                if (m_LuaScripts.Count >= LuaNames.Count)
                {
                    // 所有Lua文件加载完成后
                    InitDone?.Invoke();
                    LuaNames.Clear();
                    LuaNames = null;
                }
            });
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 将本地LuaPath下所有Lua文件内容加入m_LuaScripts中
    /// </summary>
    void EditorLoadLuaScript()
    {
        string[] luaFiles = System.IO.Directory.GetFiles(PathUtil.LuaPath, "*.bytes", SearchOption.AllDirectories);
        for (int i = 0; i < luaFiles.Length; i++)
        {
            string fileName = PathUtil.GetStandardPath(luaFiles[i]);
            byte[] file = System.IO.File.ReadAllBytes(fileName);
            AddLuaScript(PathUtil.GetUnityPath(fileName), file);
        }
        InitDone?.Invoke();
    }
#endif

    private void Update()
    {
        if (LuaEnv != null)
        {
            // Lua GC
            LuaEnv.Tick();
        }
    }

    private void OnDestroy()
    {
        if (LuaEnv != null)
        {
            LuaEnv.Dispose();
            LuaEnv = null;
        }
    }
}
