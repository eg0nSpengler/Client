using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// ReSharper disable once CheckNamespace
public static class Build 
{
    [MenuItem("Build/Build and Run Server/Windows", false, 11)]
    public static void BuildRunServerWindows()
    {
        BuildServerWindows();
        RunServerWindows();
    }
    [MenuItem("Build/Build and Run Server/Linux", false, 13)]
    public static void BuildRunServerLinux()
    {
        BuildServerLinux();
        RunServerLinux();
    }
    
    [MenuItem("Build/Run Server/Windows", false, 21)]
    public static void RunServerWindows()
    {
        Run("SS3D_Server.exe", BuildTarget.StandaloneWindows64);
    }
    [MenuItem("Build/Run Server/Linux", false, 23)]
    public static void RunServerLinux()
    {
        Run( "SS3D_Server", BuildTarget.StandaloneLinux64);
    }
    
    [MenuItem("Build/Server/Windows", false, 31)]
    public static void BuildServerWindows()
    {
        Compile("Assets/Scenes/Server.unity", "SS3D_Server.exe", true, BuildTarget.StandaloneWindows64);
    }
    [MenuItem("Build/Server/OSX", false, 32)]
    public static void BuildServerOsx()
    {
        Compile("Assets/Scenes/Server.unity", "SS3D_Server", true, BuildTarget.StandaloneOSX);
    }
    [MenuItem("Build/Server/Linux", false, 33)]
    public static void BuildServerLinux()
    {
        Compile("Assets/Scenes/Server.unity", "SS3D_Server", true, BuildTarget.StandaloneLinux64);
    }
    [MenuItem("Build/Client/Windows", false, 41)]
    public static void BuildClientWindows()
    {
        Compile("Assets/Scenes/Client.unity", "SS3D.exe", false, BuildTarget.StandaloneWindows64);
    }
    [MenuItem("Build/Client/OSX", false, 42)]
    public static void BuildClientOsx()
    {
        Compile("Assets/Scenes/Client.unity", "SS3D", false, BuildTarget.StandaloneOSX);
    }
    [MenuItem("Build/Client/Linux", false, 43)]
    public static void BuildClientLinux()
    {
        Compile("Assets/Scenes/Client.unity", "SS3D", false, BuildTarget.StandaloneLinux64);
    }

    private static void Compile(string scene, string filename, bool server, BuildTarget platform)
    {
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] { scene },
            locationPathName = $"Build/{(server ? "Server" : "Client")}/{platform}/{filename}",
            target = platform,
            options = server ? BuildOptions.StrictMode | BuildOptions.EnableHeadlessMode | BuildOptions.Development : BuildOptions.StrictMode | BuildOptions.Development,
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        switch (summary.result)
        {
            case BuildResult.Succeeded:
                Debug.Log("Build succeeded");
                break;
            case BuildResult.Failed:
                Debug.Log("Build failed");
                break;
            case BuildResult.Unknown:
                Debug.Log("Build unknown");
                break;
            case BuildResult.Cancelled:
                Debug.Log("Build cancelled");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void Run(string file, BuildTarget platform)
    {
        var path = Path.GetFullPath($"./Build/Server/{platform}/{file}");
        Debug.Log($@"Running ""{path}""");
        System.Diagnostics.Process.Start(path);
    }
}