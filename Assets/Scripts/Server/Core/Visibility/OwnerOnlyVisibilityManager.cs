namespace Server {
  public class OwnerOnlyVisibilityManager : VisibilityManager {
    protected new void Start() {
      base.Start();

      this.GetLogger().Assert(
        PlayerOwnedBehaviour.GetOwnerPlayer(gameObject) != null,
        this + " expected " + transform + " to be owned by a player.");
    }
  }
}
