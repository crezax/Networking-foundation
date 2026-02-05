using System.Collections;
using Transport.Messages;
using UnityEngine;

namespace Client {
  public class Player : MonoBehaviour {
    public static Player Instance { get; private set; }

    public delegate void OnPlayerSpawnedDelegate();
    public OnPlayerSpawnedDelegate OnPlayerSpawned;

    public long PlayerId { get; private set; }

    protected void Awake() {
      if (Instance != null) {
        Destroy(gameObject);
        return;
      }

      Instance = this;
      DontDestroyOnLoad(gameObject);
    }

    protected void Start() {
      Mixer.Instance.RegisterCallback(SpecialObjectIds.CLIENT_PLAYER_OBJECT_ID, HandlePlayerInfoMessage);
      Mixer.Instance.RegisterCallback(SpecialObjectIds.CLIENT_PLAYER_OBJECT_ID, HandlePlayerSpawnedMessage);
    }

    private void HandlePlayerInfoMessage(PlayerInfoMessage message) {
      PlayerId = message.PlayerId;
    }

    private void HandlePlayerSpawnedMessage(PlayerSpawnedMessage message) {
      StartCoroutine(NotifyOnPlayerSpawnedSubscribers());
    }

    private IEnumerator NotifyOnPlayerSpawnedSubscribers() {
      yield return null;
      OnPlayerSpawned?.Invoke();
    }
  }
}
