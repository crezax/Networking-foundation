namespace Server {
  public class ConsiderOwner : ConsiderationRule {
    private PlayerOwnedBehaviour player;

    public ConsiderOwner(PlayerOwnedBehaviour player) : base() {
      this.player = player;
      player.OnOwnerChanged += HandleOwnerChange;
      if (player.PlayerId != 0) {
        ConsiderPlayer(player.PlayerId);
      }
    }

    ~ConsiderOwner() {
      player.OnOwnerChanged -= HandleOwnerChange;
    }

    private void HandleOwnerChange(long newPlayerId, long oldPlayerId) {
      if (newPlayerId != 0) {
        ConsiderPlayer(newPlayerId);
      }
      if (oldPlayerId != 0) {
        ForgetPlayer(oldPlayerId);
      }
    }
  }
}