using System.Collections.Generic;
using System.Linq;
using Google.FlatBuffers;
using Transport.Messages;

namespace Server {
  public class TestNetworkClient : AbstractNetworkClient {

    private Dictionary<int, Client.AbstractNetworkClient> activeConnections;

    private Dictionary<int, List<ServerMessage>> dataSent = new();

    public static new TestNetworkClient Instance {
      get {
        return AbstractNetworkClient.Instance as TestNetworkClient;
      }
    }

    public override void Disconnect(int connectionId) {
      HandleDisconnection(connectionId);
    }

    public override void Send(int clientId, byte[] data) {
      if (activeConnections.TryGetValue(clientId, out Client.AbstractNetworkClient connection)) {
        connection.OnServerMessage?.Invoke(ServerMessage.GetRootAsServerMessage(new ByteBuffer(data)));

        if (!dataSent.ContainsKey(clientId)) {
          dataSent[clientId] = new();
        }
        dataSent[clientId].Add(ServerMessage.GetRootAsServerMessage(new ByteBuffer(data)));
      }
    }

    public List<ServerMessage> GetDataSent(long playerId) {
      int clientId = PlayerRegistry.Instance.GetConnectionId(playerId);
      if (!dataSent.ContainsKey(clientId)) {
        return new();
      }

      return new(dataSent[clientId]);
    }

    public ServerMessage GetLastDataSent(long playerId) {
      int clientId = PlayerRegistry.Instance.GetConnectionId(playerId);
      if (!dataSent.ContainsKey(clientId)) {
        return new();
      }

      return dataSent[clientId][^1];
    }

    public void ResetDataSent(long playerId) {
      int clientId = PlayerRegistry.Instance.GetConnectionId(playerId);
      dataSent[clientId] = new();
    }

    public void ResetDataSent() {
      dataSent = new();
    }

    protected override void StartClient() {
      activeConnections = new Dictionary<int, Client.AbstractNetworkClient>();
    }

    public void HandleConnection(int connectionId, Client.AbstractNetworkClient connection) {
      activeConnections.Add(connectionId, connection);
      OnConnect?.Invoke(connectionId);
    }

    public void HandleDisconnection(int connectionId) {
      activeConnections.Remove(connectionId);
      OnDisconnect?.Invoke(connectionId);
    }
  }
}