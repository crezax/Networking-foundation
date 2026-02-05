using UnityEditor;
using UnityEngine;

namespace Editor {
  public class Builder {
    [MenuItem("File/Build/Client with local server")]
    static void BuildClientWithLocalServer() {
      BuildPipeline.BuildPlayer(DefaultClientOptions);
    }

    [MenuItem("File/Build/Server")]
    static void BuildServer() {
      BuildPipeline.BuildPlayer(DefaultServerOptions);
    }

    private static BuildPlayerOptions DefaultClientOptions {
      get {
        var options = new BuildPlayerOptions {
          scenes = new[] {
            "Assets/Scenes/LoginScene.unity",
          },
          options = BuildOptions.AutoRunPlayer | BuildOptions.Development | BuildOptions.PatchPackage,
        };

        if (Application.platform == RuntimePlatform.WindowsEditor) {
          options.locationPathName = "Builds/Client/Client.exe";
          options.target = BuildTarget.StandaloneWindows64;
        } else if (Application.platform == RuntimePlatform.OSXEditor) {
          options.locationPathName = "Builds/Client/Client.app";
          options.target = BuildTarget.StandaloneOSX;
        } else if (Application.platform == RuntimePlatform.LinuxEditor) {
          options.locationPathName = "Builds/Client/Client.x86_64";
          options.target = BuildTarget.StandaloneLinux64;
        }
        return options;
      }
    }

    private static BuildPlayerOptions DefaultServerOptions {
      get {
        var options = new BuildPlayerOptions {
          scenes = new[] {
            "Assets/Scenes/ServerScene.unity",
          },
          options = BuildOptions.AutoRunPlayer | BuildOptions.Development | BuildOptions.PatchPackage,
        };

        if (Application.platform == RuntimePlatform.WindowsEditor) {
          options.locationPathName = "Builds/Server/Server.exe";
          options.target = BuildTarget.StandaloneWindows64;
        } else if (Application.platform == RuntimePlatform.OSXEditor) {
          options.locationPathName = "Builds/Server/Server.app";
          options.target = BuildTarget.StandaloneOSX;
        } else if (Application.platform == RuntimePlatform.LinuxEditor) {
          options.locationPathName = "Builds/Server/Server.x86_64";
          options.target = BuildTarget.StandaloneLinux64;
        }
        return options;
      }
    }
  }
}
