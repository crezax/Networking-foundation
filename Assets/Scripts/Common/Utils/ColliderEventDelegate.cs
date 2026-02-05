using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ColliderEventDelegate : MonoBehaviour {
  public delegate void TriggerEnterDelegate(Collider collider);
  public TriggerEnterDelegate HandleTriggerEnter;

  public delegate void TriggerExitDelegate(Collider collider);
  public TriggerExitDelegate HandleTriggerExit;

  public bool ColliderEnabled {
    get => col.enabled;
    set => col.enabled = value;
  }

  private Collider col;

  protected void Awake() {
    col = GetComponent<Collider>();
  }

  protected void OnTriggerEnter(Collider collider) => HandleTriggerEnter?.Invoke(collider);
  protected void OnTriggerExit(Collider collider) => HandleTriggerExit?.Invoke(collider);
}
