using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public class BuildTool : Editor
{
    [MenuItem("Tools/Build Windows Bundle")]
    static void BundleWindowsBuild()
    {
        Build(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Tools/Build Android Bundle")]
    static void BundleAndroidBuild()
    {
        Build(BuildTarget.Android);
    }

    [MenuItem("Tools/Build IOS Bundle")]
    static void BundleIOSBuild()
    {
        Build(BuildTarget.iOS);
    }

    static void Build(BuildTarget targetPlatform)
    {
        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();

        // �ļ���Ϣ�б�
        // �ļ���Ϣ��ʽ���ļ�·���� | bundle�� | �����ļ��б�
        List<string> bundlesInfo = new List<string>();

        string[] files = Directory.GetFiles(PathUtil.BuildResourcesPath, "*", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta"))
                continue;
            AssetBundleBuild assetBundle = new AssetBundleBuild();

            string fileName = PathUtil.GetStandardPath(files[i]);
            Debug.Log("file:" + fileName);

            string assetName = PathUtil.GetUnityPath(fileName);
            assetBundle.assetNames = new string[] { assetName };
            string bundleName = fileName.Replace(PathUtil.BuildResourcesPath, "").ToLower();
            assetBundle.assetBundleName = bundleName + ".ab";
            assetBundleBuilds.Add(assetBundle);

            // �����ļ���Ϣ�ַ���
            List<string> dependencyInfo = GetPrefabDependencies(assetName);
            string bundleInfo = assetName + "|" + bundleName + ".ab";

            if (dependencyInfo.Count > 0)
            {
                bundleInfo = bundleInfo + "|" + string.Join("|", dependencyInfo);
            }

            bundlesInfo.Add(bundleInfo);
        }

        // ��մ�����Ŀ¼
        if (Directory.Exists(PathUtil.BundleOutPath))
            Directory.Delete(PathUtil.BundleOutPath, true);
        Directory.CreateDirectory(PathUtil.BundleOutPath);

        BuildPipeline.BuildAssetBundles(PathUtil.BundleOutPath, assetBundleBuilds.ToArray(), BuildAssetBundleOptions.None, targetPlatform);

        // �ļ���Ϣ�б�д��filelist.txt
        File.WriteAllLines(PathUtil.BundleOutPath + "/" + AppConst.FileListName, bundlesInfo);

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// ��ȡPrefab�������ļ��б�
    /// </summary>
    /// <param name="curFile"></param>
    /// <returns></returns>
    static List<string> GetPrefabDependencies(string curFile)
    {
        List<string> dependencies = new List<string>();
        string[] files = AssetDatabase.GetDependencies(curFile);
        dependencies = files.Where(file => !file.EndsWith(".cs") && !file.Equals(curFile) && !file.StartsWith("Packages")).ToList();
        return dependencies;
    }
}
