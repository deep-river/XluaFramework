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

        // �ļ����ݣ��ȴ��ص�д��
        public DownloadHandler fileData;
    }

    /// <summary>
    /// ���ص����ļ�
    /// </summary>
    /// <param name="info">DownloadFileInfo���ļ���Ϣ</param>
    /// <param name="Complete">������ɻص�����</param>
    /// <returns></returns>
    IEnumerator DownloadFile(DownloadFileInfo info, Action<DownloadFileInfo> Complete)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);
        yield return webRequest.SendWebRequest();

        if(webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("File download failed at url:" + info.url);
            yield break;
            // TODO����������
        }
        info.fileData = webRequest.downloadHandler;
        Complete?.Invoke(info);

        webRequest.Dispose();
    }

    /// <summary>
    /// ���������ļ�
    /// </summary>
    /// <param name="info">�ļ���Ϣ</param>
    /// <param name="Complete">�����ļ�������ɻص�</param>
    /// <param name="DownloadAllComplete">�����ļ�������ɻص�</param>
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
    /// ���ļ���Ϣԭ���ַ���תΪDownloadFileInfo����б�
    /// </summary>
    /// <param name="fileData">�ļ���Ϣԭ��</param>
    /// <param name="path">�ļ���д���ļ���·��</param>
    /// <returns></returns>
    private List<DownloadFileInfo> GetFilesInfo(string fileData, string path)
    {
        string content = fileData.Trim().Replace("\r", "");
        string[] files = content.Split('\n');
        List<DownloadFileInfo> downloadFilesInfo = new List<DownloadFileInfo>(files.Length);

        for (int i = 0; i < files.Length; i++)
        {
            string[] info = files[i].Split('|');

            // �����ļ���Ϣ
            DownloadFileInfo fileInfo = new DownloadFileInfo();
            fileInfo.fileName = info[1];
            fileInfo.url = Path.Combine(path, info[1]);

            downloadFilesInfo.Add(fileInfo);
        }

        return downloadFilesInfo;
    }

    private void Start()
    {
        // �ж��Ƿ���ΰ�װ
        if (IsFirstInstall())
        {
            // �ͷ���Դ�ļ�
            ReleaseResources();
        }
        else
        {
            // ������
            CheckUpdate();
        }
    }

    private bool IsFirstInstall()
    {
        // �ж�ֻ��Ŀ¼�Ƿ���ڰ汾�ļ�
        bool isExistsReadPath = FileUtil.IsExists(Path.Combine(PathUtil.ReadOnlyPath, AppConst.FileListName));

        // �ж��ɶ�дĿ¼�Ƿ���ڰ汾�ļ�
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
    /// ֻ���ļ�����FileList������ɻص���
    /// ��ȡ��Դ�ļ��б���ʼ����ȫ���ļ�
    /// </summary>
    /// <param name="file"></param>
    private void OnDownloadReadPathFileListComplete(DownloadFileInfo file)
    {
        m_ReadPathFileListData = file.fileData.data;
        List<DownloadFileInfo> filesInfo = GetFilesInfo(file.fileData.text, PathUtil.ReadOnlyPath);
        StartCoroutine(DownloadFile(filesInfo, OnReleaseFileComplete, OnReleaseAllFileComplete));
    }

    /// <summary>
    /// �����ļ�release��ɻص���
    /// ������ɺ�д��ֻ���ļ���
    /// </summary>
    /// <param name="fileInfo"></param>
    private void OnReleaseFileComplete(DownloadFileInfo fileInfo)
    {
        Debug.LogFormat("File release complete:[{0}]", fileInfo.url);
        // �ļ�д��·��
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    /// <summary>
    /// �����ļ�release��ɻص���
    /// ����FileList�ļ���������
    /// </summary>
    private void OnReleaseAllFileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ReadPathFileListData);
        CheckUpdate();
    }

    private void CheckUpdate()
    {
        // ��ԴUrl
        string url = Path.Combine(AppConst.ResourcesUrl, AppConst.FileListName);
        DownloadFileInfo info = new DownloadFileInfo();
        info.url = url;
        StartCoroutine(DownloadFile(info, OnDownloadServerFileListComplete));
    }

    /// <summary>
    /// ��������FileList��ȡ��ɺ�ص���
    /// ��ȡ��Դ�ļ��б���ʼ����ȫ���ļ�
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

            // �жϱ����Ƿ�����ͬ���ļ�
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
    /// �����ļ�������ɻص���
    /// д���ļ�
    /// </summary>
    /// <param name="fileInfo"></param>
    private void OnUpdateFileComplete(DownloadFileInfo fileInfo)
    {
        Debug.LogFormat("File update complete:[{0}]", fileInfo.url);
        // �ļ�д��·��
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    /// <summary>
    /// �����ļ�������ɻص���
    /// ����д��FileList�ļ���������Ϸ
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
