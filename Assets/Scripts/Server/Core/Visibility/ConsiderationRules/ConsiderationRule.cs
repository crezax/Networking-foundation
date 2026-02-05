using System.Collections.Generic;

namespace Server {
  public abstract class ConsiderationRule : IVisibilityChangeEmitter {
    public OnVisibilityChangeDelegate OnVisibilityChange { get; set; }
    public HashSet<long> ConsideredPlayers { get; private set; }

    protected virtual bool ReactToDisconnects { get { return true; } }

    protected ConsiderationRule() {
      ConsideredPlayers = new HashSet<long>();

      if (ReactToDisconnects) {
        PlayerRegistry.Instance.OnPlayerUnregistered += ForgetPlayer;
      }
    }

    ~ConsiderationRule() {
      if (ReactToDisconnects) {
        PlayerRegistry.Instance.OnPlayerUnregistered -= ForgetPlayer;
      }
    }

    protected void ConsiderPlayer(long playerId) {
      ConsideredPlayers.Add(playerId);
      OnVisibilityChange?.Invoke(playerId, true);
    }

    protected void ForgetPlayer(long playerId) {
      if (!ConsideredPlayers.Contains(playerId)) {
        return;
      }
      ConsideredPlayers.Remove(playerId);
      OnVisibilityChange?.Invoke(playerId, false);
    }

    protected void ConsiderPlayer(Player player) => ConsiderPlayer(player.PlayerId);

    protected void ForgetPlayer(Player player) => ForgetPlayer(player.PlayerId);
  }
}