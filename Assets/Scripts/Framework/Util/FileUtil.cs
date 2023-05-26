using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileUtil
{
    /// <summary>
    /// 检测文件是否存在
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsExists(string path)
    {
        FileInfo file = new FileInfo(path);
        return file.Exists;
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    public static void WriteFile(string path, byte[] data)
    {
        // 标准文件路径
        path = PathUtil.GetStandardPath(path);
        // 文件夹路径
        string dir = path.Substring(0, path.LastIndexOf('/'));

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        FileInfo file = new FileInfo(path);
        if (file.Exists)
        {
            file.Delete(); 
        }
        try
        {
            // 文件流写入
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length); 
                fs.Close();
            }
        }
        catch (IOException e)
        {
            Debug.LogError(e.Message);
        }
    }
}