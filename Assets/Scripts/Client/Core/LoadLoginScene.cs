using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client {
  public class LoadLoginScene : MonoBehaviour {
    void Start() {
      SceneManager.LoadScene("LoginScene");
    }
  }
}