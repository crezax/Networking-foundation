using System;
using System.Collections.Generic;
using Google.FlatBuffers;
using Transport.Messages;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Server {
  public class NetworkedBehaviour : MonoBehaviour {
    private static long networkBehaviourCount = 1;

    public VisibilityManager VisibilityManager { get; private set; }
    [SerializeField]
    private AssetReference clientPrefab = null;

    public delegate void OnObjectSpawnedDelegate(long playerId);
    public OnObjectSpawnedDelegate OnObjectSpawned;

    public static readonly Dictionary<long, GameObject> Index = new();
    public static void RegisterSpecialObjectId(long objectId, GameObject gameObject) {
      Index.Add(objectId, gameObject);
    }
    public static void UnregisterSpecialObjectId(long objectId) {
      Index.Remove(objectId);
    }

    public static bool TryGetBehaviour<T>(long objectId, out T behaviour) where T : MonoBehaviour {
      if (Index.TryGetValue(objectId, out GameObject gameObject)) {
        behaviour = gameObject.GetComponent<T>();
        return behaviour != null;
      }
      behaviour = null;
      return false;
    }

    [SerializeField]
    private long serverId;
    public long ServerId {
      get => serverId;
      private set => serverId = value;
    }

    protected void Awake() {
      VisibilityManager = GetComponentInChildren<VisibilityManager>();
      this.GetLogger().Assert(VisibilityManager != null, gameObject + " must have IVisibilityManager");
      VisibilityManager.OnVisibilityChange += OnVisibilityChange;

      ServerId = networkBehaviourCount;
      networkBehaviourCount++;

      Index.Add(ServerId, gameObject);
    }

    protected void OnDestroy() {
      Index.Remove(ServerId);

      FlatBufferBuilder builder = new(1024);
      Offset<DespawnMessage> despawnMessage =
          DespawnMessage.CreateDespawnMessage(builder, ServerId);

      SendToAll(
          builder,
          SpecialObjectIds.CLIENT_SPAWN_MANAGER_ID,
          ServerMessageContent.DespawnMessage,
          despawnMessage);
    }

    public void SendToAll<T>(
        FlatBufferBuilder builder,
        ServerMessageContent messageType,
        Offset<T> messageOffset) where T : struct {
      SendToAll(builder, ServerId, messageType, messageOffset);
    }

    public void SendToAll<T>(
        FlatBufferBuilder builder,
        long objectId,
        ServerMessageContent messageType,
        Offset<T> messageOffset) where T : struct {
      Offset<ServerMessage> serverMessage = ServerMessage.CreateServerMessage(
          builder,
          objectId,
          messageType,
          messageOffset.Value);
      builder.Finish(serverMessage.Value);

      foreach (long playerId in VisibilityManager.VisibleToPlayers) {
        AbstractNetworkClient.Instance.Send(
          PlayerRegistry.Instance.GetConnectionId(playerId),
          builder.SizedByteArray());
      }
    }

    /** 
     * NOTE: This function will ignore the visibility system and always send the message to the 
     * requested player.
     * 
     * There are some limited cases where this is useful. For example initial spawn message.
     * In most cases though, you should use SendToAll and make sure that the object is visible to
     * correct set of players.
     *
     * Use this function only if you know what you are doing.
     */
    public void SendToPlayer_UseWithCare<T>(
        long playerId,
        FlatBufferBuilder builder,
        ServerMessageContent messageType,
        Offset<T> messageOffset) where T : struct {
      SendToPlayer(playerId, builder, ServerId, messageType, messageOffset);
    }

    public void SendInitMessage<T>(
        long playerId,
        FlatBufferBuilder builder,
        ServerMessageContent messageType,
        Offset<T> messageOffset) where T : struct {
      SendToPlayer(playerId, builder, ServerId, messageType, messageOffset);
    }

    public void SendToPlayer<T>(
        long playerId,
        FlatBufferBuilder builder,
        long recipientObjectId,
        ServerMessageContent messageType,
        Offset<T> messageOffset) where T : struct {
      Offset<ServerMessage> serverMessage = ServerMessage.CreateServerMessage(
          builder,
          recipientObjectId,
          messageType,
          messageOffset.Value);
      builder.Finish(serverMessage.Value);

      AbstractNetworkClient.Instance.Send(
          PlayerRegistry.Instance.GetConnectionId(playerId),
          builder.SizedByteArray());
    }

    private void OnVisibilityChange(long playerId, bool isVisible) {
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);
      if (isVisible) {
        StringOffset prefabIdOffset = builder.CreateString(clientPrefab.RuntimeKey.ToString());
        SpawnMessage.StartSpawnMessage(builder);
        SpawnMessage.AddServerId(builder, ServerId);
        SpawnMessage.AddPrefabId(builder, prefabIdOffset);
        SpawnMessage.AddPosition(builder, transform.position.ToTransport(builder));
        SpawnMessage.AddRotation(builder, transform.rotation.ToTransport(builder));

        SendToPlayer(
            playerId,
            builder,
            SpecialObjectIds.CLIENT_SPAWN_MANAGER_ID,
            ServerMessageContent.SpawnMessage,
            SpawnMessage.EndSpawnMessage(builder));
        OnObjectSpawned?.Invoke(playerId);
      } else {
        Offset<DespawnMessage> despawnMessage =
            DespawnMessage.CreateDespawnMessage(builder, ServerId);

        SendToPlayer(
            playerId,
            builder,
            SpecialObjectIds.CLIENT_SPAWN_MANAGER_ID,
            ServerMessageContent.DespawnMessage,
            despawnMessage);
      }
    }
  }
}
