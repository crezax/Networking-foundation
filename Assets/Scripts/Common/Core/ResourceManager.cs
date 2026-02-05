using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager {
  private static Dictionary<string, AsyncOperationHandle> assetIdToPreloadOperation = new Dictionary<string, AsyncOperationHandle>();
  private static ILogger logger = Debug.unityLogger;
  private static readonly bool shouldLog = false;

  public static T GetPreloadedAsset<T>(string assetId) where T : Object => GetPreloadedAsset<T>(new AssetReference(assetId));

  public static T GetPreloadedAsset<T>(AssetReference reference) where T : Object => UnwrapOperation<T>(GetLoadOperation(reference), reference);

  public static AsyncOperationHandle<T> LoadAssetAsync<T>(string path) {
    return Addressables.LoadAssetAsync<T>(new AssetReference(path));
  }

  public static AsyncOperationHandle<T> LoadAssetAsync<T>(string path, System.Action<T> callback) {
    AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(new AssetReference(path));
    handle.Completed += (op) => {
      callback(op.Result);
    };
    return handle;
  }

  public static AsyncOperationHandle<T> LoadAssetAsync<T>(AssetReference reference) {
    return Addressables.LoadAssetAsync<T>(reference);
  }

  public static AsyncOperationHandle<GameObject> InstantiateAsync(AssetReference reference, Transform parent = null) {
    return Addressables.InstantiateAsync(reference, parent);
  }

  public static AsyncOperationHandle<Object> PreloadAsset(string assetId) => PreloadAsset(new AssetReference(assetId));

  public static AsyncOperationHandle<Object> PreloadAsset(AssetReference reference) {
    Log("Preloading asset: {0}", reference.RuntimeKey);
    AsyncOperationHandle<Object> operation = GetLoadOperation(reference);
    assetIdToPreloadOperation[reference.RuntimeKey.ToString()] = operation;

    return operation;
  }

  public static AsyncOperationHandle<IList<Object>> PreloadAssets(string label) {
    Log("Preloading assets: {0}", label);
    AsyncOperationHandle<IList<Object>> operation =
        Addressables.LoadAssetsAsync<Object>(label, obj => {
          Log("Preloaded {0} from {1}", obj, label);
        });

    if (assetIdToPreloadOperation.ContainsKey(label)) {
      UnloadAsset(label);
    }

    assetIdToPreloadOperation[label] = operation;

    return operation;
  }

  public static void UnloadAsset(string assetId) {
    if (!assetIdToPreloadOperation.ContainsKey(assetId)) {
      LogWarning("Assets with ID: {0} doesn't seem to be loaded.", assetId);
    }
    Addressables.Release(assetIdToPreloadOperation[assetId]);
    assetIdToPreloadOperation.Remove(assetId);
  }

  private static AsyncOperationHandle<Object> GetLoadOperation(AssetReference reference) {
    if (assetIdToPreloadOperation.ContainsKey(reference.RuntimeKey.ToString())) {
      return assetIdToPreloadOperation[reference.RuntimeKey.ToString()].Convert<Object>();
    }
    if (reference.OperationHandle.IsValid() && reference.OperationHandle.IsDone) {
      return reference.OperationHandle.Convert<Object>();
    }
    return LoadAssetAsync<Object>(reference);
  }

  private static void Log(string format, params object[] args) {
    if (shouldLog) {
      logger.Log("ResourceManager", string.Format(format, args));
    }
  }

  private static void LogError(string format, params object[] args) {
    if (shouldLog) {
      logger.LogError("ResourceManager", string.Format(format, args));
    }
  }

  private static void LogWarning(string format, params object[] args) {
    if (shouldLog) {
      logger.LogWarning("ResourceManager", string.Format(format, args));
    }
  }

  private static T UnwrapOperation<T>(AsyncOperationHandle<Object> operation, AssetReference reference) where T : Object => UnwrapOperation<T>(operation, reference.RuntimeKey.ToString());

  private static T UnwrapOperation<T>(AsyncOperationHandle<Object> operation, string key) where T : Object {
    switch (operation.Status) {
      case AsyncOperationStatus.Failed:
        LogError("Failed to synchronously instantiate asset {0}. Exception: {1}", key, operation.OperationException);
        break;
      case AsyncOperationStatus.None:
        if (!assetIdToPreloadOperation.ContainsKey(key)) {
          LogError("Failed to synchronously instantiate asset {0}. Did you preload it with ResourceManager.PreloadAsset?", key);
        } else {
          LogError("Failed to synchronously instantiate asset {0}. It may have been unloaded outside of ResourceManager.", key);
        }
        break;
    }

    return operation.Result as T;
  }

  private ResourceManager() { }
}
