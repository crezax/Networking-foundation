// This file is auto generated. Do NOT edit manually. Edit MixerGenerator.cs instead.
using System;
using System.Collections.Generic;
using Transport.Messages;
using UnityEngine;

namespace Server {
  public class Mixer : MonoBehaviour {
    private readonly Dictionary<long, LoginMessageDelegate> loginMessageCallbacks = new();
    public delegate void LoginMessageDelegate(LoginMessage message);
    private readonly Dictionary<long, LoginMessageDelegateWithConnectionId> loginMessageCallbacksWithConnectionId = new();
    public delegate void LoginMessageDelegateWithConnectionId(int connectionId, LoginMessage message);

    private static Mixer instance;

    public static Mixer Instance {
      get {
        return instance;
      }
    }

    protected void Awake() {
      if (instance != null) {
        Destroy(gameObject);
        return;
      }

      instance = this;
    }

    protected void Start() {
      AbstractNetworkClient.Instance.OnClientMessage += OnClientMessage;
    }

    public void RegisterCallback(long objectId, LoginMessageDelegate callback) {
      loginMessageCallbacks[objectId] = Delegate.Combine(loginMessageCallbacks.GetValueOrDefault(objectId, null), callback) as LoginMessageDelegate;
    }
    public void RegisterCallback(long objectId, LoginMessageDelegateWithConnectionId callback) {
      loginMessageCallbacksWithConnectionId[objectId] = Delegate.Combine(loginMessageCallbacksWithConnectionId.GetValueOrDefault(objectId, null), callback) as LoginMessageDelegateWithConnectionId;
    }
    public void UnregisterCallback(long objectId, LoginMessageDelegate callback) {
      if (loginMessageCallbacks.ContainsKey(objectId)) {
        loginMessageCallbacks[objectId] = Delegate.Remove(loginMessageCallbacks[objectId], callback) as LoginMessageDelegate;
      }
    }
    public void UnregisterCallback(long objectId, LoginMessageDelegateWithConnectionId callback) {
      if (loginMessageCallbacksWithConnectionId.ContainsKey(objectId)) {
        loginMessageCallbacksWithConnectionId[objectId] = Delegate.Remove(loginMessageCallbacksWithConnectionId[objectId], callback) as LoginMessageDelegateWithConnectionId;
      }
    }

    private void OnClientMessage(int connectionId, ClientMessage message) {
      GameObject obj;

      if (!NetworkedBehaviour.Index.TryGetValue(message.ServerId, out obj)) {
        this.GetLogger().LogWarning("{0} sent message {1} while recipient ({2}) doesn't exist", connectionId, message.MessageType, message.ServerId);
        return;
      }

      PlayerOwnedBehaviour playerOwnedBehaviour =
        obj.GetComponent<PlayerOwnedBehaviour>();
      Debug.Assert(
        obj.GetComponent<IAnyPlayerMessageReceiver>() != null || (playerOwnedBehaviour != null && playerOwnedBehaviour.CanReceiveMessages(
          PlayerRegistry.Instance.GetPlayerId(connectionId))),
        "Player " + connectionId + " sent a message to " + obj + " despite not " +
        "having ownership. This might be caused by recent ownership change, " +
        "bug or a cheating attempt.");

      if (obj.GetComponent<IAnyPlayerMessageReceiver>() == null && (playerOwnedBehaviour == null || !playerOwnedBehaviour.CanReceiveMessages(
          PlayerRegistry.Instance.GetPlayerId(connectionId)))) {
        return;
      }

      switch (message.MessageType) {
        case ClientMessageContent.LoginMessage:
          LoginMessageDelegate loginMessageDelegate;
          if (loginMessageCallbacks.TryGetValue(message.ServerId, out loginMessageDelegate)) {
            loginMessageDelegate?.Invoke(
              message.Message<LoginMessage>().Value);
          }
          LoginMessageDelegateWithConnectionId loginMessageDelegateWithConnectionId;
          if (loginMessageCallbacksWithConnectionId.TryGetValue(message.ServerId, out loginMessageDelegateWithConnectionId)) {
            loginMessageDelegateWithConnectionId?.Invoke(
              connectionId,
              message.Message<LoginMessage>().Value);
          }
          break;
      }
    }
  }
}