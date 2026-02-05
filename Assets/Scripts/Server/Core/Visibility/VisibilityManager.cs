using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Server {
  public class VisibilityManager : MonoBehaviour, IVisibilityChangeEmitter {
    [SerializeField]
    private long[] visibleToPlayers = null;
    [SerializeField]
    private long[] consideredPlayers = null;

    public OnVisibilityChangeDelegate OnVisibilityChange { get; set; }

    public HashSet<long> VisibleToPlayers { get; private set; }

    private HashSet<ConsiderationRule> ConsiderationRules { get; set; }
    private Dictionary<long, long> ConsiderationCount { get; set; }
    public HashSet<long> ConsideredPlayers {
      get {
        return new HashSet<long>(
          ConsiderationCount
            .Where(entry => entry.Value > 0)
            .Select(entry => entry.Key)
            .ToList());
      }
    }
    private HashSet<IVisibilityRule> VisibilityRules { get; set; }

    private static Dictionary<long, HashSet<VisibilityManager>> managersConsideringPlayer =
      new Dictionary<long, HashSet<VisibilityManager>>();

    public static void ReinitializeVisibility(long playerId) {
      HashSet<VisibilityManager> managers;

      if (!managersConsideringPlayer.TryGetValue(playerId, out managers)) {
        return;
      }

      foreach (VisibilityManager manager in managers) {
        manager.RecalculateVisibility(playerId, /* forceEmitForVisibleObjects */ true);
      }
    }

    protected void Awake() {
      VisibleToPlayers = new HashSet<long>();
      ConsiderationRules = new HashSet<ConsiderationRule>();
      ConsiderationCount = new Dictionary<long, long>();
      VisibilityRules = new HashSet<IVisibilityRule>();
    }

    protected void Start() {
      PlayerOwnedBehaviour pob = PlayerOwnedBehaviour.GetOwnerPlayer(gameObject);
      if (pob != null) {
        AddConsideredPlayersProvider(new ConsiderOwner(pob));
      }
    }

    protected void Update() {
      foreach (long playerId in ConsideredPlayers) {
        RecalculateVisibility(playerId);
      }
      visibleToPlayers = VisibleToPlayers.ToArray();
      consideredPlayers = ConsideredPlayers.ToArray();
    }

    protected void OnDestroy() {
      foreach (long playerId in ConsideredPlayers) {
        managersConsideringPlayer[playerId].Remove(this);
      }
    }

    public void ApplyVisibilityRule(IVisibilityRule rule) {
      VisibilityRules.Add(rule);
    }

    public void RemoveVisibilityRule(IVisibilityRule rule) {
      VisibilityRules.Remove(rule);
    }

    public void AddConsideredPlayersProvider(ConsiderationRule provider) {
      ConsiderationRules.Add(provider);

      provider.OnVisibilityChange += OnProviderVisibilityChange;
      provider.ConsideredPlayers.ToList().ForEach(playerId => ConsiderPlayer(playerId));
    }

    public void RemoveConsideredPlayersProvider(ConsiderationRule provider) {
      ConsiderationRules.Remove(provider);

      provider.OnVisibilityChange -= OnProviderVisibilityChange;
      provider.ConsideredPlayers.ToList().ForEach(playerId => ForgetPlayer(playerId));
    }

    public void ClearConsideredPlayersProviders() {
      while (ConsiderationRules.Count > 0) {
        RemoveConsideredPlayersProvider(ConsiderationRules.First());
      }
    }

    private void OnProviderVisibilityChange(long playerId, bool isVisible) {
      if (isVisible) {
        ConsiderPlayer(playerId);
      } else {
        ForgetPlayer(playerId);
      }
    }

    protected void ConsiderPlayer(long playerId) {
      if (!ConsiderationCount.ContainsKey(playerId)) {
        ConsiderationCount[playerId] = 0;
      }
      if (!managersConsideringPlayer.ContainsKey(playerId)) {
        managersConsideringPlayer.Add(playerId, new HashSet<VisibilityManager>());
      }

      managersConsideringPlayer[playerId].Add(this);
      ConsideredPlayers.Add(playerId);
      ConsiderationCount[playerId]++;
    }

    protected void ForgetPlayer(long playerId) {
      if (!ConsiderationCount.ContainsKey(playerId)) {
        this.GetLogger().LogWarning("ForgetPlayer called on player that was not considered");
        return;
      }

      ConsiderationCount[playerId]--;
      if (ConsiderationCount[playerId] > 0) {
        return;
      }

      ConsiderationCount.Remove(playerId);
      managersConsideringPlayer[playerId].Remove(this);
      ConsideredPlayers.Remove(playerId);
      SetVisibility(playerId, false);
    }

    private void RecalculateVisibility(long playerId) {
      RecalculateVisibility(playerId, false);
    }

    private void RecalculateVisibility(long playerId, bool forceEmitForVisibleObjects) {
      foreach (IVisibilityRule rule in VisibilityRules) {
        if (!rule.IsVisible(transform.root.gameObject, playerId)) {
          SetVisibility(playerId, false);
          return;
        }
      }
      SetVisibility(playerId, true, forceEmitForVisibleObjects);
    }

    private void SetVisibility(long playerId, bool newIsVisible) => SetVisibility(playerId, newIsVisible, false);
    private void SetVisibility(long playerId, bool newIsVisible, bool forceEmit) {
      bool currentIsVisible = VisibleToPlayers.Contains(playerId);

      if (currentIsVisible == newIsVisible && !forceEmit) {
        return;
      }

      if (newIsVisible) {
        VisibleToPlayers.Add(playerId);
      } else {
        VisibleToPlayers.Remove(playerId);
      }

      if (OnVisibilityChange != null) {
        OnVisibilityChange(playerId, newIsVisible);
      }
    }
  }
}
