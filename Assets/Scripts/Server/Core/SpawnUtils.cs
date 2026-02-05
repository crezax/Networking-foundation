using System;
using System.Linq;
using UnityEngine;

namespace Server {
  public class SpawnUtils {
    /// Executes action once after all objects are spawned for the player.
    public static void ExecuteOnceAfterSpawned(long playerId, Action action, params GameObject[] gameObjects) {
      ExecuteOnceAfterSpawned(
          playerId,
          action,
          gameObjects.Select(go => go.GetComponent<NetworkedBehaviour>()).ToArray());
    }

    /// Executes action once after all objects are spawned for the player.
    public static void ExecuteOnceAfterSpawned(long playerId, Action action, params Component[] components) {
      ExecuteOnceAfterSpawned(
          playerId,
          action,
          components.Select(c => c.GetComponent<NetworkedBehaviour>()).ToArray());
    }

    /// Executes action once after all objects are spawned for the player.
    public static void ExecuteOnceAfterSpawned(long playerId, Action action, params NetworkedBehaviour[] networkedBehaviours) {
      int spawned = 0;
      foreach (NetworkedBehaviour networkedBehaviour in networkedBehaviours) {
        NetworkedBehaviour.OnObjectSpawnedDelegate callback = null;
        callback = (spawnedForPlayerId) => {
          if (playerId != spawnedForPlayerId) {
            return;
          }
          networkedBehaviour.OnObjectSpawned -= callback;

          spawned++;
          if (spawned == networkedBehaviours.Length) {
            action.Invoke();
          }
        };
        networkedBehaviour.OnObjectSpawned += callback;
      }
    }

    /// Executes action once after all objects are spawned for the player.
    public static void ExecuteOnceAfterSpawned(long playerId, Action<long> action, params GameObject[] gameObjects) {
      ExecuteOnceAfterSpawned(playerId, () => action.Invoke(playerId), gameObjects);
    }

    /// Executes action once after all objects are spawned for the player.
    public static void ExecuteOnceAfterSpawned(long playerId, Action<long> action, params Component[] components) {
      ExecuteOnceAfterSpawned(playerId, () => action.Invoke(playerId), components);
    }

    /// Executes action once after all objects are spawned for the player.
    public static void ExecuteOnceAfterSpawned(long playerId, Action<long> action, params NetworkedBehaviour[] networkedBehaviours) {
      ExecuteOnceAfterSpawned(playerId, () => action.Invoke(playerId), networkedBehaviours);
    }

    private SpawnUtils() { }
  }
}
