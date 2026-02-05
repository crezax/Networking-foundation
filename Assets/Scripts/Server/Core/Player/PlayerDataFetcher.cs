using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.FlatBuffers;
using Transport.Messages;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Server {
  [RequireComponent(typeof(NetworkedBehaviour))]
  public class PlayerDataFetcher : MonoBehaviour, IAnyPlayerMessageReceiver {
    private NetworkedBehaviour networkedBehaviour;

    private Dictionary<long, GameObject> playerToPlayerCharacter;

    private static PlayerDataFetcher Instance { get; set; }

    protected void Awake() {
      if (Instance != null) {
        Destroy(gameObject);
        return;
      }

      Instance = this;
      networkedBehaviour = GetComponent<NetworkedBehaviour>();
      playerToPlayerCharacter = new Dictionary<long, GameObject>();
    }

    protected void Start() {
      NetworkedBehaviour.RegisterSpecialObjectId(SpecialObjectIds.SERVER_LOGIN_RECIPIENT_ID, gameObject);
      Mixer.Instance.RegisterCallback(SpecialObjectIds.SERVER_LOGIN_RECIPIENT_ID, HandlePlayerLogin);
      PlayerRegistry.Instance.OnPlayerUnregistered += HandlePlayerLogout;
    }

    protected void OnDestroy() {
      NetworkedBehaviour.UnregisterSpecialObjectId(SpecialObjectIds.SERVER_LOGIN_RECIPIENT_ID);
      Mixer.Instance.UnregisterCallback(SpecialObjectIds.SERVER_LOGIN_RECIPIENT_ID, HandlePlayerLogin);
    }

    private void HandlePlayerLogin(int connectionId, LoginMessage loginMessage) {
      Debug.Log("HandlePlayerLogin");
      // TODO: Fetch the real data from somewhere
      long playerId = (long)loginMessage.SessionToken.GetHashCode();
      PlayerRegistry.Instance.RegisterPlayer(
          connectionId,
          new Player(playerId, loginMessage.SessionToken));

      SendPlayerIdToThePlayer(playerId);
    }

    private void HandlePlayerLogout(Player player) {
      if (!playerToPlayerCharacter.TryGetValue(player.PlayerId, out GameObject playerCharacter)) {
        // If there is an issue fetching data from API, we will disconnect player without ever spawning a character.
        return;
      }

      playerToPlayerCharacter.Remove(player.PlayerId);
    }

    private void SendPlayerIdToThePlayer(long playerId) {
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);
      Offset<PlayerInfoMessage> playerInfoMessage =
          PlayerInfoMessage.CreatePlayerInfoMessage(builder, playerId);

      networkedBehaviour.SendToPlayer(
          playerId,
          builder,
          SpecialObjectIds.CLIENT_PLAYER_OBJECT_ID,
          ServerMessageContent.PlayerInfoMessage,
          playerInfoMessage);
    }
  }
}
