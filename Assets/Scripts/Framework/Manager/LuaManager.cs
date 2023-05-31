using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Windows;
using XLua;

public class LuaManager : MonoBehaviour
{
    // ����lua�ļ��б�
    public List<string> LuaNames = new List<string>();

    // lua�ļ����ݻ���
    private Dictionary<string, byte[]> m_LuaScripts;

    // Lua�����
    public LuaEnv LuaEnv;

    // Lua�ļ�������ɻص�
    Action InitDone;

    public void Init(Action init)
    {
        InitDone += init;

        // Lua�������ʼ��
        LuaEnv = new LuaEnv();

        // �����Զ���Loader
        LuaEnv.AddLoader(Loader);

        // m_LuaScripts��ʼ��
        m_LuaScripts = new Dictionary<string, byte[]>();

#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
            EditorLoadLuaScript();
        else
#endif
            LoadLuaScript();
    }

    /// <summary>
    /// Lua�����ִ��Lua�ļ�
    /// ����ļ���Lua�ű���ʽ�ǣ����������һ��DoString("require 'main'")��Ȼ����main.lua���������ű�������lua�ű���������ִ�У�lua main.lua��
    /// </summary>
    /// <param name="name"></param>
    public void StartLua(string name)
    {
        LuaEnv.DoString(string.Format("require '{0}'", name));
    }

    /// <summary>
    /// �Զ���Loader
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    byte[] Loader(ref string name)
    {
        return GetLuaScript(name);
    }

    /// <summary>
    /// ��m_LuaScripts�в���ָ��Lua�ļ�
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public byte[] GetLuaScript(string name)
    {
        // ����·��תΪ�ļ�·��
        // ����require ui.login.register -> ui/login/register.bytes
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
    /// ��LuaNames������Lua�ļ����ݼ���m_LuaScripts��
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
                    // ����Lua�ļ�������ɺ�
                    InitDone?.Invoke();
                    LuaNames.Clear();
                    LuaNames = null;
                }
            });
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// ������LuaPath������Lua�ļ����ݼ���m_LuaScripts��
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
