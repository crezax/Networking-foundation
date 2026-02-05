using System.Linq;

namespace Server {
  public class InheritConsideredPlayers : ConsiderationRule {
    private IVisibilityChangeEmitter Parent { get; set; }

    // The parent should already be listening to player registry events
    protected override bool ReactToDisconnects => false;

    public InheritConsideredPlayers(IVisibilityChangeEmitter parent) : base() {
      Parent = parent;
      Parent.OnVisibilityChange += OnParentVisibilityChange;
      ConsideredPlayers.UnionWith(Parent.ConsideredPlayers);
      Parent.ConsideredPlayers.ToList()
        .ForEach(playerId => ConsideredPlayers.Add(playerId));
    }

    ~InheritConsideredPlayers() {
      Parent.OnVisibilityChange -= OnParentVisibilityChange;
    }

    private void OnParentVisibilityChange(long playerId, bool isVisibile) {
      OnVisibilityChange?.Invoke(playerId, isVisibile);
    }
  }
}