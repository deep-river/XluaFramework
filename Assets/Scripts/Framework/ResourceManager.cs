using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UObject = UnityEngine.Object;

public class ResourceManager : MonoBehaviour
{
    // �ļ���Ϣ��ʽ���ļ�·���� | bundle�� | �����ļ�1 [| �����ļ�2 | ... ]
    internal class BundleInfo
    {
        public string AssetsName;
        public string BundleName;
        public List<string> Dependencies;
    }

    // �洢BundleInfo
    private Dictionary<string, BundleInfo> m_BundlesInfo = new Dictionary<string, BundleInfo>();

    /// <summary>
    /// �����汾�ļ�
    /// </summary>
    private void ParseVersionFile()
    {
        // �汾�ļ�·��
        string url = Path.Combine(PathUtil.BundleResourcePath, AppConst.FileListName);
        string[] data = File.ReadAllLines(url);

        for (int i = 0; i < data.Length; i++)
        {
            BundleInfo bundleInfo = new BundleInfo();
            string[] info = data[i].Split('|');
            bundleInfo.AssetsName = info[0];
            bundleInfo.BundleName = info[1];
            // List���ԣ����������飬���ɶ�̬����
            bundleInfo.Dependencies = new List<string>(info.Length - 2);

            for (int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependencies.Add(info[j]);
            }

            m_BundlesInfo.Add(bundleInfo.AssetsName, bundleInfo);
        }
    }

    /// <summary>
    /// �첽������Դ
    /// </summary>
    /// <param name="assetName">��Դ�ļ���</param>
    /// <param name="action">��ɻص�</param>
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

    public void LoadAsset(string assetName, Action<UObject> action)
    {
        StartCoroutine(LoadBundleAsync(assetName, action));
    }



    /// <summary>
    /// Test AssetLoad
    /// </summary>
    void Start()
    {
        ParseVersionFile();
        string assetName = "Assets/BuildResources/UI/Prefab/UITest.prefab";
        this.LoadAsset(assetName, OnComplete);
    }

    private void OnComplete(UObject obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }
}
