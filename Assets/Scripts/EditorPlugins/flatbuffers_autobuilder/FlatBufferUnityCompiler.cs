#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Linq;

public class FlatBufferUnityCompiler : AssetPostprocessor {

  private static readonly string ROOT_DIR = Path.Combine(Path.GetFullPath(Application.dataPath), "Scripts", "Transport");

  public static bool logError {
    get {
      return true;
    }
  }

  public static bool logStandard {
    get {
      return true;
    }
  }

  public static string excPath {
    get {
      var flatc = "flatc";
      if (Application.platform == RuntimePlatform.WindowsEditor)
        flatc = "flatc.exe";
      
      return Path.GetFullPath(Path.Combine(Application.dataPath, "../", flatc));
    }
  }

  static string[] AllFbsFiles {
    get {
      string[] fbsFiles = Directory.GetFiles(Application.dataPath, "*.fbs", SearchOption.AllDirectories);
      return fbsFiles;
    }
  }

  static bool anyChanges = false;

  static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
    anyChanges = false;

    foreach (string str in importedAssets) {
      anyChanges = CompileFlatBuffersAssetPath(str) || anyChanges;
    }

    if (anyChanges) {
      AssetDatabase.Refresh();
    }
  }

  private static void CompileAssets(string[] assets) {
    assets = assets.Select(asset => Directory.GetParent(Application.dataPath) + Path.DirectorySeparatorChar.ToString() + asset).ToArray();

    string options = string.Format(" -n ");

    string finalArguments = options + string.Format(" \"{0}\"", string.Join(" ", assets));

    UnityEngine.Debug.Log(finalArguments);

    ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = excPath, Arguments = finalArguments };

    Process proc = new Process() { StartInfo = startInfo };
    proc.StartInfo.UseShellExecute = false;
    proc.StartInfo.RedirectStandardOutput = true;
    proc.StartInfo.RedirectStandardError = true;
    proc.Start();

    string output = proc.StandardOutput.ReadToEnd();
    string error = proc.StandardError.ReadToEnd();
    proc.WaitForExit();

    if (logError && output != "") {
      UnityEngine.Debug.LogError("FlatBuffers Unity : " + output);
    }

    if (logError && error != "") {
      UnityEngine.Debug.LogError("FlatBuffers Unity : " + error);
    }

    if (logStandard && error == "" && output == "") {
      UnityEngine.Debug.Log("FlatBuffers Unity : Compiled " + string.Join(" ", assets.Select(asset => asset.Remove(0, Application.dataPath.Length))));
    }
  }

  private static bool CompileFlatBuffersAssetPath(string assetPath) {
    string fbsFileSystemPath = Directory.GetParent(Application.dataPath) + Path.DirectorySeparatorChar.ToString() + assetPath;

    if (Path.GetExtension(fbsFileSystemPath) != ".fbs") {
      return false;
    }

    if (!File.Exists(excPath))
    {
        UnityEngine.Debug.LogError("FlatBuffers Unity: flatc compiler not found at " + excPath + ". Please build flatc for your system and place it in the project root.");
        return false;
    }

    if (Application.platform != RuntimePlatform.WindowsEditor)
    {
        ProcessStartInfo chmodStartInfo = new ProcessStartInfo() { FileName = "/bin/chmod", Arguments = "+x \"" + excPath + "\"" };
        Process chmodProc = new Process() { StartInfo = chmodStartInfo };
        chmodProc.StartInfo.UseShellExecute = false;
        chmodProc.Start();
        chmodProc.WaitForExit();
    }

    string options = string.Format(" -n --gen-onefile --filename-suffix \"\" -o \"{0}\" -I \"{1}\" ", Directory.GetParent(fbsFileSystemPath), ROOT_DIR);

    string finalArguments = options + string.Format(" \"{0}\"", fbsFileSystemPath);

    UnityEngine.Debug.Log(finalArguments);

    ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = excPath, Arguments = finalArguments };

    Process proc = new Process() { StartInfo = startInfo };
    proc.StartInfo.UseShellExecute = false;
    proc.StartInfo.RedirectStandardOutput = true;
    proc.StartInfo.RedirectStandardError = true;
    proc.Start();

    string output = proc.StandardOutput.ReadToEnd();
    string error = proc.StandardError.ReadToEnd();
    proc.WaitForExit();

    if (logError && output != "") {
      UnityEngine.Debug.LogError("FlatBuffers Unity : " + output);
    }

    if (logError && error != "") {
      UnityEngine.Debug.LogError("FlatBuffers Unity : " + error);
    }

    if (logStandard && error == "" && output == "") {
      UnityEngine.Debug.Log("FlatBuffers Unity : Compiled " + fbsFileSystemPath.Remove(0, Application.dataPath.Length));
    }
    return true;
  }
}

#endif
