using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.FlatBuffers;
using Transport.Messages;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Client {
  [RequireComponent(typeof(NetworkedBehaviour))]
  public class SessionManager : MonoBehaviour {
    public enum State {
      PreConnection,
      Connecting,
      LoadingAssets,
      Authenticating,
      CharacterSelection,
      Connected,
      Error,
    }

    public static SessionManager Instance { get; private set; }

    public delegate void OnStateChangeDelegate(State state);
    public OnStateChangeDelegate OnStateChange;

    public string Error { get; private set; }

    private State state;
    public State CurrentState {
      get => state;
      private set {
        this.GetLogger().Log("SessionManager state change: {0}", value);
        state = value;
        OnStateChange?.Invoke(value);
      }
    }

    private readonly List<AsyncOperationHandle> loadingOperations = new List<AsyncOperationHandle>();
    public float AssetLoadProgress {
      get {
        if (loadingOperations.Count == 0) {
          return 1;
        }
        return loadingOperations.Sum(op => op.PercentComplete) / loadingOperations.Count;
      }
    }

    public string AuthToken { get; private set; }
    public string Username { get; private set; }

    private bool manualDisconnect = false;
    private long selectedCharacterId;

    public void Disconnect() {
      manualDisconnect = true;
      AbstractNetworkClient.Instance.Disconnect();
    }

    public void LogIn(string username, string password) {
      Username = username;
      CurrentState = State.Connecting;

      StartCoroutine(LogInAsync(password));
    }

    protected void Awake() {
      if (Instance != null) {
        Destroy(gameObject);
        return;
      }

      Instance = this;
      CurrentState = State.PreConnection;
      this.GetLogger().Enabled = true;
      DontDestroyOnLoad(gameObject);
    }

    protected void Start() {
      AbstractNetworkClient.Instance.OnConnect += OnConnected;
      AbstractNetworkClient.Instance.OnDisconnect += GoToLoginScreen;
    }

    private void GoToLoginScreen() {
      Error = manualDisconnect ? "" : "Disconnected";
      CurrentState = manualDisconnect ? State.PreConnection : State.Error;
      manualDisconnect = false;
      if (SceneManager.GetActiveScene().name == "LoginScene") {
        return;
      }
      this.GetLogger().Log("Navigating to LoginScene");
      SceneManager.LoadScene("LoginScene");
    }

    private void OnConnected() {
      if (SceneManager.GetActiveScene().name == "ClientDemoScene") {
        return;
      }
      StartCoroutine(OnConnectedAsync());
    }

    private IEnumerator OnConnectedAsync() {
      this.GetLogger().Log("Loading required assets");
      yield return PreloadAssets("ClientCore");

      this.GetLogger().Log("Navigating to ClientDemoScene");
      SceneManager.LoadScene("ClientDemoScene");
      SendLoginMessage();
    }

    private IEnumerator LogInAsync(string password) {
      yield return null;
      AbstractNetworkClient.Instance.Connect();
    }

    private IEnumerator PreloadAssets(string label) {
      loadingOperations.Clear();
      loadingOperations.Add(ResourceManager.PreloadAssets(label));
      CurrentState = State.LoadingAssets;

      foreach (var loadingOperation in loadingOperations) {
        if (!loadingOperation.IsDone) {
          yield return loadingOperation;
        }
      }
    }

    private void SendLoginMessage() {
      CurrentState = State.Authenticating;
      // TODO: Let's encrypt this or something please
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);
      Offset<LoginMessage> message = LoginMessage.CreateLoginMessage(
          builder, builder.CreateString(AuthToken), selectedCharacterId);

      GetComponent<NetworkedBehaviour>().Send(
        SpecialObjectIds.SERVER_LOGIN_RECIPIENT_ID,
        builder,
        ClientMessageContent.LoginMessage,
        message);
      CurrentState = State.Connected;
    }
  }
}
