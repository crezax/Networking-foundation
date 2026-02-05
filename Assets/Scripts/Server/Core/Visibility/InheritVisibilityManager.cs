namespace Server {
  public class InheritVisibilityManager : VisibilityManager {
    private IVisibilityChangeEmitter parent;
    public IVisibilityChangeEmitter Parent {
      get {
        return parent;
      }
      set {
        parent = value;
        ClearConsideredPlayersProviders();
        AddConsideredPlayersProvider(
            new InheritConsideredPlayers(parent));
      }
    }
  }
}
