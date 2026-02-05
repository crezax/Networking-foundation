namespace Server {
  public class ConsiderAllPlayers : ConsiderationRule {
    public ConsiderAllPlayers() : base() {
      PlayerRegistry.Instance.AllPlayers.ForEach(player => ConsiderPlayer(player));
      PlayerRegistry.Instance.OnPlayerRegistered += ConsiderPlayer;
    }

    ~ConsiderAllPlayers() {
      PlayerRegistry.Instance.OnPlayerRegistered -= ConsiderPlayer;
    }
  }
}
