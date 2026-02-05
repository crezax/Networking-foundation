using UnityEditor;
using System.IO;
using UnityEngine;

public class CreateAssetBundles {
  [MenuItem("Assets/Build AssetBundles")]
  static void BuildAllAssetBundles() {
    string assetBundleDirectory = SpawnManagerConstants.ASSET_BUNDLE_DIRECTORY;
    if (!Directory.Exists(assetBundleDirectory)) {
      Directory.CreateDirectory(assetBundleDirectory);
    }
    BuildPipeline.BuildAssetBundles(
      assetBundleDirectory,
      BuildAssetBundleOptions.ChunkBasedCompression,
      BuildTarget.StandaloneWindows);
  }
}
