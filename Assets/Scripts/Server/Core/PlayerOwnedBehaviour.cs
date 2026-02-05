using Google.FlatBuffers;
using Transport.Messages;
using UnityEngine;

namespace Server {
  [RequireComponent(typeof(NetworkedBehaviour))]
  public class PlayerOwnedBehaviour : MonoBehaviour {
    [SerializeField]
    private long playerId;
    public long PlayerId {
      get => playerId;
      set {
        long oldPlayerId = playerId;
        playerId = value;

        OnOwnerChanged?.Invoke(playerId, oldPlayerId);

        BroadcastPlayerId();
      }
    }

    public delegate void OwnerChangedDelegate(long newPlayerId, long oldPlayerId);
    public OwnerChangedDelegate OnOwnerChanged;

    private NetworkedBehaviour networkedBehaviour;

    public static PlayerOwnedBehaviour GetOwnerPlayer(GameObject go) {
      return go.transform.root.GetComponent<PlayerOwnedBehaviour>();
    }

    public static PlayerOwnedBehaviour GetOwnerPlayer(MonoBehaviour mb) {
      return GetOwnerPlayer(mb.gameObject);
    }

    protected void Awake() {
      networkedBehaviour = GetComponent<NetworkedBehaviour>();

      networkedBehaviour.OnObjectSpawned += OnObjectSpawned;
    }

    public bool CanReceiveMessages(long playerId) {
      return PlayerId == playerId;
    }

    private void BroadcastPlayerId() {
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);

      Offset<PlayerOwnershipMessage> playerOwnershipMessage =
          PlayerOwnershipMessage.CreatePlayerOwnershipMessage(builder, (long)PlayerId);

      networkedBehaviour.SendToAll(
          builder,
          ServerMessageContent.PlayerOwnershipMessage,
          playerOwnershipMessage);
    }


    private void OnObjectSpawned(long playerId) {
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);

      Offset<PlayerOwnershipMessage> playerOwnershipMessage =
          PlayerOwnershipMessage.CreatePlayerOwnershipMessage(builder, (long)PlayerId);

      networkedBehaviour.SendInitMessage(
          playerId,
          builder,
          ServerMessageContent.PlayerOwnershipMessage,
          playerOwnershipMessage);
    }
  }
}
