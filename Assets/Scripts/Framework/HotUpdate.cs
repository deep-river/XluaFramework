using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class HotUpdate : MonoBehaviour
{
    byte[] m_ReadPathFileListData;

    byte[] m_ServerFileListData;

    internal class DownloadFileInfo
    {
        public string url;
        public string fileName;

        // 文件内容，等待回调写入
        public DownloadHandler fileData;
    }

    /// <summary>
    /// 下载单个文件
    /// </summary>
    /// <param name="info">DownloadFileInfo类文件信息</param>
    /// <param name="Complete">下载完成回调方法</param>
    /// <returns></returns>
    IEnumerator DownloadFile(DownloadFileInfo info, Action<DownloadFileInfo> Complete)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);
        yield return webRequest.SendWebRequest();

        if(webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("File download failed at url:" + info.url);
            yield break;
            // TODO：重试下载
        }
        info.fileData = webRequest.downloadHandler;
        Complete?.Invoke(info);

        webRequest.Dispose();
    }

    /// <summary>
    /// 批量下载文件
    /// </summary>
    /// <param name="info">文件信息</param>
    /// <param name="Complete">单个文件下载完成回调</param>
    /// <param name="DownloadAllComplete">所有文件下载完成回调</param>
    /// <returns></returns>
    IEnumerator DownloadFile(List<DownloadFileInfo> filesInfo, Action<DownloadFileInfo> Complete, Action DownloadAllComplete)
    {
        foreach (DownloadFileInfo info in filesInfo)
        {
            yield return DownloadFile(info, Complete);
        }
        DownloadAllComplete?.Invoke();
    }

    /// <summary>
    /// 将文件信息原文字符串转为DownloadFileInfo类的列表
    /// </summary>
    /// <param name="fileData">文件信息原文</param>
    /// <param name="path">文件待写入文件夹路径</param>
    /// <returns></returns>
    private List<DownloadFileInfo> GetFilesInfo(string fileData, string path)
    {
        string content = fileData.Trim().Replace("\r", "");
        string[] files = content.Split('\n');
        List<DownloadFileInfo> downloadFilesInfo = new List<DownloadFileInfo>(files.Length);

        for (int i = 0; i < files.Length; i++)
        {
            string[] info = files[i].Split('|');

            // 构造文件信息
            DownloadFileInfo fileInfo = new DownloadFileInfo();
            fileInfo.fileName = info[1];
            fileInfo.url = Path.Combine(path, info[1]);

            downloadFilesInfo.Add(fileInfo);
        }

        return downloadFilesInfo;
    }

    private void Start()
    {
        // 判断是否初次安装
        if (IsFirstInstall())
        {
            // 释放资源文件
            ReleaseResources();
        }
        else
        {
            // 检查更新
            CheckUpdate();
        }
    }

    private bool IsFirstInstall()
    {
        // 判断只读目录是否存在版本文件
        bool isExistsReadPath = FileUtil.IsExists(Path.Combine(PathUtil.ReadOnlyPath, AppConst.FileListName));

        // 判读可读写目录是否存在版本文件
        bool isExistsReadWritePath = FileUtil.IsExists(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName));

        return isExistsReadPath && !isExistsReadWritePath;
    }

    private void ReleaseResources()
    {
        string url = Path.Combine(PathUtil.ReadOnlyPath, AppConst.FileListName);
        DownloadFileInfo info = new DownloadFileInfo();
        info.url = url;
        StartCoroutine(DownloadFile(info, OnDownloadReadPathFileListComplete));
    }

    /// <summary>
    /// 只读文件夹下FileList下载完成回调：
    /// 获取资源文件列表并开始下载全部文件
    /// </summary>
    /// <param name="file"></param>
    private void OnDownloadReadPathFileListComplete(DownloadFileInfo file)
    {
        m_ReadPathFileListData = file.fileData.data;
        List<DownloadFileInfo> filesInfo = GetFilesInfo(file.fileData.text, PathUtil.ReadOnlyPath);
        StartCoroutine(DownloadFile(filesInfo, OnReleaseFileComplete, OnReleaseAllFileComplete));
    }

    /// <summary>
    /// 单个文件release完成回调：
    /// 下载完成后写入只读文件夹
    /// </summary>
    /// <param name="fileInfo"></param>
    private void OnReleaseFileComplete(DownloadFileInfo fileInfo)
    {
        Debug.LogFormat("File release complete:[{0}]", fileInfo.url);
        // 文件写入路径
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    /// <summary>
    /// 所有文件release完成回调：
    /// 创建FileList文件并检查更新
    /// </summary>
    private void OnReleaseAllFileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ReadPathFileListData);
        CheckUpdate();
    }

    private void CheckUpdate()
    {
        // 资源Url
        string url = Path.Combine(AppConst.ResourcesUrl, AppConst.FileListName);
        DownloadFileInfo info = new DownloadFileInfo();
        info.url = url;
        StartCoroutine(DownloadFile(info, OnDownloadServerFileListComplete));
    }

    /// <summary>
    /// 服务器上FileList拉取完成后回调：
    /// 获取资源文件列表并开始下载全部文件
    /// </summary>
    /// <param name="file"></param>
    private void OnDownloadServerFileListComplete(DownloadFileInfo file)
    {
        m_ServerFileListData = file.fileData.data;

        List<DownloadFileInfo> filesInfo = GetFilesInfo(file.fileData.text, AppConst.ResourcesUrl);

        List<DownloadFileInfo> downloadListFiles = new List<DownloadFileInfo>();

        for (int i = 0; i < filesInfo.Count; i++)
        {
            string localFile = Path.Combine(PathUtil.ReadWritePath, filesInfo[i].fileName);

            // 判断本地是否已有同名文件
            if (!FileUtil.IsExists(localFile))
            {
                filesInfo[i].url = Path.Combine(AppConst.ResourcesUrl, filesInfo[i].fileName);
                 downloadListFiles.Add(filesInfo[i]);
            }
        }

        if (downloadListFiles.Count > 0)
        {
            StartCoroutine(DownloadFile(filesInfo, OnUpdateFileComplete, OnUpdateAllFileComplete));
        }
        else
        {
            EnterGame();
        }
    }

    /// <summary>
    /// 单个文件更新完成回调：
    /// 写入文件
    /// </summary>
    /// <param name="fileInfo"></param>
    private void OnUpdateFileComplete(DownloadFileInfo fileInfo)
    {
        Debug.LogFormat("File update complete:[{0}]", fileInfo.url);
        // 文件写入路径
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    /// <summary>
    /// 所有文件更新完成回调：
    /// 更新写入FileList文件并进入游戏
    /// </summary>
    private void OnUpdateAllFileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ServerFileListData);
        EnterGame();
    }

    private void EnterGame()
    {
        Manager.Resource.ParseVersionFile();
        Manager.Resource.LoadUI("UITest", OnComplete);
        Manager.Resource.LoadUI("Login/Popup_Login", OnComplete);
    }

    private void OnComplete(UnityEngine.Object obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }
}
