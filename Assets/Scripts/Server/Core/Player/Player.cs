using UnityEngine;

namespace Server {
  public class Player {
    public long PlayerId { get; private set; }
    public string Name { get; private set; }
    public PlayerCharacter PlayerCharacter { get; set; }

    public Player(long playerId, string name) => (PlayerId, Name) = (playerId, name);
  }
}
