using System;
using System.Collections.Generic;
using Google.FlatBuffers;
using Transport.Messages;

namespace Server {
  public class TelepathyNetworkClient : AbstractNetworkClient {

    private int Port => 8888;
    private Telepathy.Server server;
    private HashSet<int> activeConnections;

    public override void Disconnect(int connectionId) {
      server.Disconnect(connectionId);
      HandleDisconnection(connectionId);
    }

    public override void Send(int clientId, byte[] data) {
      server.Send(clientId, data);
    }

    protected override void StartClient() {
      activeConnections = new HashSet<int>();
      server = new Telepathy.Server(16000);
      server.Start(Port);
      server.OnConnected += HandleConnection;
      server.OnData += HandleData;
      server.OnDisconnected += HandleDisconnection;
    }

    private void HandleConnection(int connectionId, string connectionAddress) {
      activeConnections.Add(connectionId);

      OnConnect?.Invoke(connectionId);
    }

    private void HandleData(int connectionId, ArraySegment<byte> data) {
      HandleClientMessage(
        connectionId,
        ClientMessage.GetRootAsClientMessage(
          new ByteBuffer(data.Array, data.Offset)));
    }

    private void HandleClientMessage(int connectionId, ClientMessage message) {
      OnClientMessage(connectionId, message);
    }

    private void HandleDisconnection(int connectionId) {
      this.GetLogger().Log("Disconnected from {0}", connectionId);

      activeConnections.Remove(connectionId);
      OnDisconnect?.Invoke(connectionId);
    }
  }
}
