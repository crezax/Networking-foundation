using Transport.Messages;
using UnityEngine;

namespace Client {
  [RequireComponent(typeof(NetworkedBehaviour))]
  public class PlayerOwnedBehaviour : MonoBehaviour {
    private NetworkedBehaviour networkedBehaviour;

    public delegate void OwnerChangedDelegate(long newPlayerId, long oldPlayerId);
    public OwnerChangedDelegate OnOwnerChanged;

    [SerializeField]
    private long playerId;
    public long PlayerId {
      get => playerId;
      private set {
        long oldPlayerId = playerId;
        playerId = value;

        if (OnOwnerChanged != null) {
          OnOwnerChanged(playerId, oldPlayerId);
        }
      }
    }

    public bool IsOwnedByLocalPlayer => playerId == Player.Instance.PlayerId;

    protected void Awake() {
      networkedBehaviour = GetComponent<NetworkedBehaviour>();
      networkedBehaviour.OnServerIdSet(RegisterCallbacks);
    }

    protected void OnDestroy() {
      UnregisterCallbacks();
    }

    private void RegisterCallbacks() {
      Mixer.Instance.RegisterCallback(networkedBehaviour.ServerId, HandlePlayerOwnershipMessage);
    }

    private void UnregisterCallbacks() {
      Mixer.Instance.UnregisterCallback(networkedBehaviour.ServerId, HandlePlayerOwnershipMessage);
    }

    private void HandlePlayerOwnershipMessage(PlayerOwnershipMessage message) {
      PlayerId = message.PlayerId;
    }
  }
}
