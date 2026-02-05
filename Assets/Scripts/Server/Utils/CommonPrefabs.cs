using UnityEngine;

namespace Server {
  public class CommonPrefabs : MonoBehaviour {
    public static CommonPrefabs Instance { get; private set; }

    protected void Awake() {
      if (Instance != null) {
        Destroy(Instance.gameObject);
      }
      Instance = this;
    }
  }
}