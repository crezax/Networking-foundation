using UnityEngine;

namespace Server {
  public class PlayerCharacter : MonoBehaviour {
    public static PlayerCharacter Get(GameObject entity) {
      return entity.GetComponent<PlayerCharacter>();
    }

    private long? characterId;
    public long CharacterId {
      get {
        if (!characterId.HasValue) {
          this.GetLogger().LogError("CharacterId not set");
        }
        return characterId.Value;
      }
      set {
        if (characterId.HasValue) {
          this.GetLogger().LogError("CharacterId already set");
        } else {
          characterId = value;
        }
      }
    }
  }
}