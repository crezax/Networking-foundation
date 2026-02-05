namespace Server {
  public class ServerOnlyVisibilityManager : VisibilityManager {
    protected new void Start() {
      base.Start();

      ClearConsideredPlayersProviders();
    }
  }
}
