using System.Collections;
using System.Collections.Generic;
using Google.FlatBuffers;
using Transport.Messages;
using UnityEngine;

namespace Client {
  public class NetworkedBehaviour : MonoBehaviour {
    private bool initDone = false;

    public delegate void CallbackDelegate();
    private readonly List<CallbackDelegate> callbacks = new();

    public delegate IEnumerator DespawnDelegate();
    public DespawnDelegate OnWillDespawn;

    [SerializeField]
    private long serverId;
    public long ServerId {
      get => serverId;
      set => serverId = value;
    }

    public void Init(long serverId) {
      ServerId = serverId;
      initDone = true;

      foreach (CallbackDelegate callback in callbacks) {
        callback();
      }
      callbacks.Clear();
    }

    /**
     * You should most likely NEVER use Destroy for NetworkedBehaviours, but use Despawn instead.
     */
    public void Despawn() {
      StartCoroutine(DespawnCoroutine());
    }

    private IEnumerator DespawnCoroutine() {
      yield return OnWillDespawn?.Invoke();
      Destroy(gameObject);
    }

    /* Use this function to perform all the initialization that depends on
     * ServerID. The flow is:
     * - spawn object
     * - Awake() is called on all scripts
     * - ServerId is set
     *
     * As a result, we cannot use ServerId in Awake() and doing things like 
     * subscribing to messages in say Start() would risk missing a message that 
     * was sent immediately after SpawnMessage
     */
    public void OnServerIdSet(CallbackDelegate callback) {
      if (initDone) {
        callback();
      } else {
        callbacks.Add(callback);
      }
    }

    public void Send<T>(
      FlatBufferBuilder builder,
      ClientMessageContent messageType,
      Offset<T> messageOffset) where T : struct {

      Send(ServerId, builder, messageType, messageOffset);
    }

    public void Send<T>(
      long serverId,
      FlatBufferBuilder builder,
      ClientMessageContent messageType,
      Offset<T> messageOffset) where T : struct {

      Offset<ClientMessage> clientMessageOffset =
        ClientMessage.CreateClientMessage(
          builder,
          serverId,
          messageType,
          messageOffset.Value);
      builder.Finish(clientMessageOffset.Value);
      this.GetLogger().LogDebug("Sending data to server {0}: {1}", ServerId, messageType);
      AbstractNetworkClient.Instance.Send(builder.SizedByteArray());
    }
  }
}
