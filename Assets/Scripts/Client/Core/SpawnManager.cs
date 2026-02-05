using System.Collections.Generic;
using Transport.Messages;
using UnityEngine;

namespace Client {
  public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance { get; private set; }

    private Dictionary<long, GameObject> serverIdToInstanceId;

    public GameObject GetGameObjectByServerId(long serverId) {
      return serverIdToInstanceId.GetValueOrDefault(serverId, null);
    }

    protected void Awake() {
      if (Instance != null) {
        this.GetLogger().LogWarning("Duplicate SpawnManager created in GameObject: {0}. SpawnManager already exists on: {1}", gameObject, Instance.gameObject);
        Destroy(gameObject);
        return;
      }

      Instance = this;
      serverIdToInstanceId = new Dictionary<long, GameObject>();
      DontDestroyOnLoad(gameObject);
    }

    protected void Start() {
      Mixer.Instance.RegisterCallback(SpecialObjectIds.CLIENT_SPAWN_MANAGER_ID, HandleSpawnMessage);
      Mixer.Instance.RegisterCallback(SpecialObjectIds.CLIENT_SPAWN_MANAGER_ID, HandleDespawnMessage);
      AbstractNetworkClient.Instance.OnDisconnect += HandleDisconnect;
    }

    private void HandleDisconnect() {
      serverIdToInstanceId.Clear();
    }

    private void HandleSpawnMessage(SpawnMessage message) {
      this.GetLogger().Log("Spawning object with ServerId {0}", message.ServerId);
      GameObject spawnedObject = Instantiate(
        ResourceManager.GetPreloadedAsset<GameObject>(message.PrefabId),
        message.Position.Value.ToVector3(),
        message.Rotation.Value.ToQuaternion());
      foreach (NetworkedBehaviour nb in spawnedObject.GetComponents<NetworkedBehaviour>()) {
        nb.Init(message.ServerId);
      }
      serverIdToInstanceId.Add(message.ServerId, spawnedObject);
      this.GetLogger().Log("Spawned {0} with ID {1}", spawnedObject, message.ServerId);
    }

    private void HandleDespawnMessage(DespawnMessage message) {
      if (!serverIdToInstanceId.TryGetValue(message.ServerId, out GameObject obj) || obj == null) {
        // This should never happen I suppose? We probably never want to remove
        // network objects unless specifically instructed by the server?
        return;
      }

      obj.GetComponent<NetworkedBehaviour>().Despawn();
      serverIdToInstanceId.Remove(message.ServerId);
    }
  }
}
