using Transport.Messages;
using UnityEngine;

namespace Server {
  public abstract class AbstractNetworkClient : MonoBehaviour {
    public delegate void ConnectionDelegate(int connectionId);
    public ConnectionDelegate OnConnect;

    public delegate void DisconnectionDelegate(int connectionId);
    public DisconnectionDelegate OnDisconnect;

    public delegate void ClientMessageDelegate(int connectionId, ClientMessage message);
    public ClientMessageDelegate OnClientMessage;

    public static AbstractNetworkClient Instance { private set; get; }

    public abstract void Disconnect(int connectionId);
    public abstract void Send(int clientId, byte[] data);
    protected abstract void StartClient();

    protected void Awake() {
      if (Instance != null) {
        this.GetLogger().LogError("Network client ({0}) already exists. Destroying {1}", Instance, this);
        Destroy(gameObject);
        return;
      }

      Instance = this;
      DontDestroyOnLoad(gameObject);
    }

    protected void Start() {
      this.GetLogger().Log("Starting network client: {0}", this);
      StartClient();
    }
  }
}