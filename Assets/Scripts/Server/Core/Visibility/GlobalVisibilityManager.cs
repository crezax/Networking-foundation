namespace Server {
  public class GlobalVisibilityManager : VisibilityManager {
    protected new void Start() {
      base.Start();

      AddConsideredPlayersProvider(new ConsiderAllPlayers());
    }
  }
}
