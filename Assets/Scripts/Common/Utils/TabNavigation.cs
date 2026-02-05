using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour {

  private EventSystem eventSystem;

  protected void Start() {
    eventSystem = EventSystem.current;
  }

  protected void Update() {
    if (Input.GetKeyDown(KeyCode.Tab)) {
      Selectable current = eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
      Selectable next = (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) ? 
          current.FindSelectableOnUp() : current.FindSelectableOnDown();

      if (next != null) {
        PointerEventData eventData = new(eventSystem);

        TMP_InputField inputField = next.GetComponent<TMP_InputField>();
        if (inputField != null) {
          inputField.OnPointerClick(eventData);
        }

        eventSystem.SetSelectedGameObject(next.gameObject, new BaseEventData(eventSystem));
      }
    }
  }
}
