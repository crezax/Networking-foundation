using Google.FlatBuffers;
using Transport.Messages;

namespace Client {
  public class TestNetworkClient : AbstractNetworkClient {

    private static int id = 0;

    private int clientId;
    private Server.TestNetworkClient server;

    public static new TestNetworkClient Instance {
      get {
        return AbstractNetworkClient.Instance as TestNetworkClient;
      }
    }

    public override void Connect() {
      server = Server.AbstractNetworkClient.Instance as Server.TestNetworkClient;
      OnConnect?.Invoke();
      server.HandleConnection(clientId, this);
    }

    public override void Disconnect() {
      OnDisconnect?.Invoke();
      server.HandleDisconnection(clientId);
      server = null;
    }

    public override void Send(byte[] data) {
      server.OnClientMessage?.Invoke(clientId, ClientMessage.GetRootAsClientMessage(new ByteBuffer(data)));
    }

    protected override void StartClient() {
      clientId = id++;
    }
  }
}