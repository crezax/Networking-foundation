using System.Collections.Generic;
using UnityEngine;

namespace Server {
  public class PlayerRegistry : MonoBehaviour {
    private BiDictionary<int, long> connectionIdToPlayerId;
    private Dictionary<long, Player> playersById;

    public static PlayerRegistry Instance { get; private set; }

    public delegate void PlayerRegisteredDelegate(Player player);
    public PlayerRegisteredDelegate OnPlayerRegistered;

    public delegate void PlayerUnregisteredDelegate(Player player);
    public PlayerRegisteredDelegate OnPlayerUnregistered;

    public List<Player> AllPlayers => new List<Player>(playersById.Values);

    protected void Awake() {
      if (Instance != null) {
        Destroy(gameObject);
        return;
      }

      Instance = this;
      connectionIdToPlayerId = new BiDictionary<int, long>();
      playersById = new Dictionary<long, Player>();
    }

    protected void Start() {
      AbstractNetworkClient.Instance.OnDisconnect += UnregisterPlayer;
    }

    public void RegisterPlayer(int connectionId, Player player) {
      connectionIdToPlayerId.Add(connectionId, player.PlayerId);
      playersById.Add(player.PlayerId, player);

      if (OnPlayerRegistered != null) {
        OnPlayerRegistered(player);
      }
    }

    public long GetPlayerId(int connectionId) {
      long playerId;

      if (connectionIdToPlayerId.TryGetByFirst(connectionId, out playerId)) {
        return playerId;
      }

      return 0;
    }

    public int GetConnectionId(long playerId) {
      int connectionId;

      if (connectionIdToPlayerId.TryGetBySecond(playerId, out connectionId)) {
        return connectionId;
      }

      return 0;
    }

    private void UnregisterPlayer(int connectionId) {
      long playerId;

      if (!connectionIdToPlayerId.TryGetByFirst(connectionId, out playerId)) {
        return;
      }

      Player player = playersById[playerId];
      connectionIdToPlayerId.RemoveByFirst(connectionId);
      playersById.Remove(playerId);

      if (OnPlayerUnregistered != null) {
        OnPlayerUnregistered(player);
      }
    }
  }
}
