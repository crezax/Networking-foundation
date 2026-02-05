using System.Collections.Generic;

namespace Server {
  public delegate void OnVisibilityChangeDelegate(long playerId, bool isVisible);

  public interface IVisibilityChangeEmitter {
    OnVisibilityChangeDelegate OnVisibilityChange { get; set; }
    HashSet<long> ConsideredPlayers { get; }
  }
}