using UnityEngine;

namespace Server {
  public interface IVisibilityRule {
    bool IsVisible(GameObject obj, long playerId);
  }
}
