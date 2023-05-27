using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathUtil : MonoBehaviour
{
    // ��Ŀ¼
    public static readonly string AssetsPath = Application.dataPath;

    // ��Ҫ��Bundle��Ŀ¼
    public static readonly string BuildResourcesPath = AssetsPath + "/BuildResources/";

    // bundle���Ŀ¼
    public static readonly string BundleOutPath = Application.streamingAssetsPath;

    // ֻ��Ŀ¼
    public static readonly string ReadOnlyPath = Application.streamingAssetsPath;

    // �ɶ�дĿ¼
    public static readonly string ReadWritePath = Application.persistentDataPath;

    // Lua�ű�·��
    public static readonly string LuaPath = "Assets/BuildResources/LuaScript";

    // bundle��Դ·��
    // streamingAssetsPathΪֻ���ļ��У����ڴ��ǰ�����Դ
    // persistentDataPathΪ�ɶ�д�ļ��У����ڰ�װ���ͷ���Դ
    public static string BundleResourcePath
    {
        get 
        { 
            if (AppConst.GameMode == GameMode.UpdateMode)
            {
                return ReadWritePath;
            }
            return ReadOnlyPath; 
        }
    }

    /// <summary>
    /// ��ȡUnity�����·��
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetUnityPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }
        return path.Substring(path.IndexOf("Assets"));
    }

    /// <summary>
    /// ��ȡ��׼·��
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetStandardPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }
        return path.Trim().Replace("\\", "/");
    }

    public static string GetLuaPath(string name)
    {
        return string.Format("Assets/BuildResources/LuaScript/{0}.bytes", name);
    }

    public static string GetUIPath(string name)
    {
        return string.Format("Assets/BuildResources/UI/Prefab/{0}.prefab", name);
    }

    public static string GetMusicPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Music/{0}", name);
    }

    public static string GetSoundPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Sound/{0}", name);
    }

    public static string GetEffectPath(string name)
    {
        return string.Format("Assets/BuildResources/Effect/Prefab/{0}.prefab", name);
    }

    public static string GetSpritePath(string name)
    {
        return string.Format("Assets/BuildResources/Sprite/{0}", name);
    }

    public static string GetScenePath(string name)
    {
        return string.Format("Assets/BuildResources/Scene/{0}.unity", name);
    }
}
