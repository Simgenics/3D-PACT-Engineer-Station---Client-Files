using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ScenarioManager : MonoBehaviour
{
    public static string challengeExtension = "nodeCanvas";
    public static string questionsExtension = "questions";
    public static string testExtension = "test";
    public static string challengePath = Application.streamingAssetsPath + "/NodeCanvases/";
    public static string questionPath = Application.streamingAssetsPath + "/Questions/";
    public static string testPath = Application.streamingAssetsPath + "/Tests/";

    public static List<FileInfo> ScenarioFileInfos { get; private set; } = new List<FileInfo>();

    private void Awake()
    {
        Load();
    }

    public static void Load()
    {
        ScenarioFileInfos.Clear();
        if (Directory.Exists(challengePath))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(challengePath);
            if (dirInfo != null)
            {
                ScenarioFileInfos.AddRange(dirInfo.GetFiles($"*.{challengeExtension}"));
            }
        }
    }

    public static string GetFullPath(string path, string fileName, string extension)
    {
        return $"{path}{fileName}.{extension}";
    }
}
