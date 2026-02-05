// This file is auto generated. Do NOT edit manually. Edit MixerGenerator.cs instead.
using System;
using System.Collections.Generic;
using Transport.Messages;
using UnityEngine;

namespace Client {
  public class Mixer : MonoBehaviour {
    private readonly Dictionary<long, DespawnMessageDelegate> despawnMessageCallbacks = new();
    public delegate void DespawnMessageDelegate(DespawnMessage message);

    private readonly Dictionary<long, PlayerInfoMessageDelegate> playerInfoMessageCallbacks = new();
    public delegate void PlayerInfoMessageDelegate(PlayerInfoMessage message);

    private readonly Dictionary<long, PlayerOwnershipMessageDelegate> playerOwnershipMessageCallbacks = new();
    public delegate void PlayerOwnershipMessageDelegate(PlayerOwnershipMessage message);

    private readonly Dictionary<long, PlayerSpawnedMessageDelegate> playerSpawnedMessageCallbacks = new();
    public delegate void PlayerSpawnedMessageDelegate(PlayerSpawnedMessage message);

    private readonly Dictionary<long, SpawnMessageDelegate> spawnMessageCallbacks = new();
    public delegate void SpawnMessageDelegate(SpawnMessage message);

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
      AbstractNetworkClient.Instance.OnServerMessage += OnServerMessage;
    }
    public void RegisterCallback(long objectId, DespawnMessageDelegate callback) {
      despawnMessageCallbacks[objectId] = Delegate.Combine(despawnMessageCallbacks.GetValueOrDefault(objectId, null), callback) as DespawnMessageDelegate;
    }
    public void RegisterCallback(long objectId, PlayerInfoMessageDelegate callback) {
      playerInfoMessageCallbacks[objectId] = Delegate.Combine(playerInfoMessageCallbacks.GetValueOrDefault(objectId, null), callback) as PlayerInfoMessageDelegate;
    }
    public void RegisterCallback(long objectId, PlayerOwnershipMessageDelegate callback) {
      playerOwnershipMessageCallbacks[objectId] = Delegate.Combine(playerOwnershipMessageCallbacks.GetValueOrDefault(objectId, null), callback) as PlayerOwnershipMessageDelegate;
    }
    public void RegisterCallback(long objectId, PlayerSpawnedMessageDelegate callback) {
      playerSpawnedMessageCallbacks[objectId] = Delegate.Combine(playerSpawnedMessageCallbacks.GetValueOrDefault(objectId, null), callback) as PlayerSpawnedMessageDelegate;
    }
    public void RegisterCallback(long objectId, SpawnMessageDelegate callback) {
      spawnMessageCallbacks[objectId] = Delegate.Combine(spawnMessageCallbacks.GetValueOrDefault(objectId, null), callback) as SpawnMessageDelegate;
    }
    public void UnregisterCallback(long objectId, DespawnMessageDelegate callback) {
      if (despawnMessageCallbacks.ContainsKey(objectId)) {
        despawnMessageCallbacks[objectId] = Delegate.Remove(despawnMessageCallbacks[objectId], callback) as DespawnMessageDelegate;
      }
    }
    public void UnregisterCallback(long objectId, PlayerInfoMessageDelegate callback) {
      if (playerInfoMessageCallbacks.ContainsKey(objectId)) {
        playerInfoMessageCallbacks[objectId] = Delegate.Remove(playerInfoMessageCallbacks[objectId], callback) as PlayerInfoMessageDelegate;
      }
    }
    public void UnregisterCallback(long objectId, PlayerOwnershipMessageDelegate callback) {
      if (playerOwnershipMessageCallbacks.ContainsKey(objectId)) {
        playerOwnershipMessageCallbacks[objectId] = Delegate.Remove(playerOwnershipMessageCallbacks[objectId], callback) as PlayerOwnershipMessageDelegate;
      }
    }
    public void UnregisterCallback(long objectId, PlayerSpawnedMessageDelegate callback) {
      if (playerSpawnedMessageCallbacks.ContainsKey(objectId)) {
        playerSpawnedMessageCallbacks[objectId] = Delegate.Remove(playerSpawnedMessageCallbacks[objectId], callback) as PlayerSpawnedMessageDelegate;
      }
    }
    public void UnregisterCallback(long objectId, SpawnMessageDelegate callback) {
      if (spawnMessageCallbacks.ContainsKey(objectId)) {
        spawnMessageCallbacks[objectId] = Delegate.Remove(spawnMessageCallbacks[objectId], callback) as SpawnMessageDelegate;
      }
    }
    private void OnServerMessage(ServerMessage message) {
      switch (message.MessageType) {
        case ServerMessageContent.DespawnMessage:
          DespawnMessageDelegate despawnMessageDelegate;
          if (despawnMessageCallbacks.TryGetValue(message.ServerId, out despawnMessageDelegate)) {
            despawnMessageDelegate?.Invoke(
              message.Message<DespawnMessage>().Value);
          }
          break;
        case ServerMessageContent.PlayerInfoMessage:
          PlayerInfoMessageDelegate playerInfoMessageDelegate;
          if (playerInfoMessageCallbacks.TryGetValue(message.ServerId, out playerInfoMessageDelegate)) {
            playerInfoMessageDelegate?.Invoke(
              message.Message<PlayerInfoMessage>().Value);
          }
          break;
        case ServerMessageContent.PlayerOwnershipMessage:
          PlayerOwnershipMessageDelegate playerOwnershipMessageDelegate;
          if (playerOwnershipMessageCallbacks.TryGetValue(message.ServerId, out playerOwnershipMessageDelegate)) {
            playerOwnershipMessageDelegate?.Invoke(
              message.Message<PlayerOwnershipMessage>().Value);
          }
          break;
        case ServerMessageContent.PlayerSpawnedMessage:
          PlayerSpawnedMessageDelegate playerSpawnedMessageDelegate;
          if (playerSpawnedMessageCallbacks.TryGetValue(message.ServerId, out playerSpawnedMessageDelegate)) {
            playerSpawnedMessageDelegate?.Invoke(
              message.Message<PlayerSpawnedMessage>().Value);
          }
          break;
        case ServerMessageContent.SpawnMessage:
          SpawnMessageDelegate spawnMessageDelegate;
          if (spawnMessageCallbacks.TryGetValue(message.ServerId, out spawnMessageDelegate)) {
            spawnMessageDelegate?.Invoke(
              message.Message<SpawnMessage>().Value);
          }
          break;
      }
    }
  }
}