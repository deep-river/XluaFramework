using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UObject = UnityEngine.Object;

public class ResourceManager : MonoBehaviour
{
    // 文件信息格式：文件路径名 | bundle名 | 依赖文件1 [| 依赖文件2 | ... ]
    internal class BundleInfo
    {
        public string AssetsName;
        public string BundleName;
        public List<string> Dependencies;
    }

    // 存储BundleInfo
    private Dictionary<string, BundleInfo> m_BundlesInfo = new Dictionary<string, BundleInfo>();

    /// <summary>
    /// 解析版本文件
    /// </summary>
    private void ParseVersionFile()
    {
        // 版本文件路径
        string url = Path.Combine(PathUtil.BundleResourcePath, AppConst.FileListName);
        string[] data = File.ReadAllLines(url);

        for (int i = 0; i < data.Length; i++)
        {
            BundleInfo bundleInfo = new BundleInfo();
            string[] info = data[i].Split('|');
            bundleInfo.AssetsName = info[0];
            bundleInfo.BundleName = info[1];
            // List特性：本质是数组，但可动态扩容
            bundleInfo.Dependencies = new List<string>(info.Length - 2);

            for (int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependencies.Add(info[j]);
            }

            m_BundlesInfo.Add(bundleInfo.AssetsName, bundleInfo);
        }
    }

    /// <summary>
    /// 异步加载bundle中资源
    /// </summary>
    /// <param name="assetName">资源文件名</param>
    /// <param name="action">完成回调</param>
    /// <returns></returns>
    IEnumerator LoadBundleAsync(string assetName, Action<UObject> action = null)
    {
        string bundleName = m_BundlesInfo[assetName].BundleName;
        string bundlePath = Path.Combine(PathUtil.BundleResourcePath, bundleName);
        List<string> dependencies = m_BundlesInfo[assetName].Dependencies;

        if (dependencies != null && dependencies.Count > 0)
        {
            for (int i = 0; i < dependencies.Count; i++)
            {
                yield return LoadBundleAsync(dependencies[i]);
            }
        }

        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return request;

        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync(assetName);
        yield return bundleRequest;

        if (action != null && bundleRequest != null)
        {
            action.Invoke(bundleRequest.asset);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器模式下直接从目录下加载资源，而不从bundle中加载
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    void EditorLoadAsset(string assetName, Action<UObject> action = null)
    {
        UObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, typeof(UObject));
        if (obj == null)
            Debug.LogError("asset does not exist:" + assetName);

        action?.Invoke(obj);
    }
#endif


    /// <summary>
    /// 根据GameMode加载资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    // GameMode在场景的Root节点下修改
    private void LoadAsset(string assetName, Action<UObject> action)
    {
#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
            EditorLoadAsset(assetName, action);
        else
#endif
            StartCoroutine(LoadBundleAsync(assetName, action));
    }

    /// <summary>
    /// 对外暴露的资源调用API
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    public void LoadUI(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetUIPath(assetName), action);
    }

    public void LoadMusic(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetMusicPath(assetName), action);
    }

    public void LoadSound(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetSoundPath(assetName), action);
    }

    public void LoadEffect(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetEffectPath(assetName), action);
    }

    public void LoadScene(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetScenePath(assetName), action);
    }



    /// <summary>
    /// Test AssetLoad
    /// </summary>
    void Start()
    {
        ParseVersionFile();
        LoadUI("UITest", OnComplete);
        LoadUI("Login/Popup_Login", OnComplete);
    }

    private void OnComplete(UObject obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }
}
