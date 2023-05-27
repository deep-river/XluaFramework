using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathUtil : MonoBehaviour
{
    // 根目录
    public static readonly string AssetsPath = Application.dataPath;

    // 需要打Bundle的目录
    public static readonly string BuildResourcesPath = AssetsPath + "/BuildResources/";

    // bundle输出目录
    public static readonly string BundleOutPath = Application.streamingAssetsPath;

    // 只读目录
    public static readonly string ReadOnlyPath = Application.streamingAssetsPath;

    // 可读写目录
    public static readonly string ReadWritePath = Application.persistentDataPath;

    // Lua脚本路径
    public static readonly string LuaPath = "Assets/BuildResources/LuaScript";

    // bundle资源路径
    // streamingAssetsPath为只读文件夹，用于打包前存放资源
    // persistentDataPath为可读写文件夹，用于安装后释放资源
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
    /// 获取Unity的相对路径
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
    /// 获取标准路径
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
