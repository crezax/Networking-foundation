using Transport.Messages;
using UnityEngine;

namespace Client {
  public abstract class AbstractNetworkClient : MonoBehaviour {

    public delegate void ConnectionDelegate();
    public ConnectionDelegate OnConnect;

    public delegate void DisconnectionDelegate();
    public DisconnectionDelegate OnDisconnect;

    public delegate void ServerMessageDelegate(ServerMessage message);
    public ServerMessageDelegate OnServerMessage;

    public static AbstractNetworkClient Instance { get; private set; }

    public abstract void Connect();
    public abstract void Disconnect();
    public abstract void Send(byte[] data);
    protected abstract void StartClient();

    // Use this for initialization
    protected void Awake() {
      if (Instance != null) {
        this.GetLogger().LogError("Network client ({0}) already exists. Destroying {1}", Instance, this);
        Destroy(gameObject);
        return;
      }

      Instance = this;
      DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    protected void Start() {
      this.GetLogger().Log("Starting network client: {0}", this);
      StartClient();
    }
  }
}