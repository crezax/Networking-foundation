using UnityEngine;

namespace Client {
  [RequireComponent(typeof(NetworkedBehaviour))]
  public abstract class ClientBehaviour : MonoBehaviour {
    protected abstract void RegisterCallbacks();
    protected abstract void UnregisterCallbacks();

    public NetworkedBehaviour NetworkedBehaviour { get; protected set; }

    protected void Awake() {
      NetworkedBehaviour = GetComponent<NetworkedBehaviour>();
      NetworkedBehaviour.OnServerIdSet(RegisterCallbacks);
    }

    protected void OnDestroy() {
      UnregisterCallbacks();
    }
  }
}