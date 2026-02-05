using System.Collections.Generic;
using Google.FlatBuffers;
using Transport.Messages;
using UnityEngine.SceneManagement;

namespace Client {
  public class TelepathyNetworkClient : AbstractNetworkClient {

    private int HostId { get; set; }
    public bool Connected { get; private set; }
    private bool manualDisconnect = false;
    private Telepathy.Client client;

    private readonly HashSet<ServerMessageContent> messageTypesBlacklistedForLogging =
        new HashSet<ServerMessageContent>();

    private string Host => "127.0.0.1";
    private int Port => 8888;

    public override void Connect() {
      if (Connected) {
        return;
      }

      client.Connect(Host, Port);
    }

    public override void Disconnect() {
      if (!Connected) {
        return;
      }

      manualDisconnect = true;
      client.Disconnect();
    }

    public override void Send(byte[] data) {
      client.Send(data);
    }

    protected override void StartClient() {
      client = new Telepathy.Client(16000);
      client.OnConnected += HandleConnection;
      client.OnData += HandleServerData;
      client.OnDisconnected += HandleDisconnection;
    }

    private void HandleServerData(System.ArraySegment<byte> data) {
      HandleServerMessage(
        ServerMessage.GetRootAsServerMessage(new ByteBuffer(data.Array, data.Offset)));
    }

    private void HandleServerMessage(ServerMessage message) {
      if (!messageTypesBlacklistedForLogging.Contains(message.MessageType)) {
        this.GetLogger().LogDebug("Received {0} for {1}", message.MessageType, message.ServerId);
      }
      OnServerMessage?.Invoke(message);
    }

    private void HandleConnection() {
      this.GetLogger().Log("Connected to {0}:{1} with hostId: {2}", Host, Port, HostId);
      Connected = true;

      OnConnect?.Invoke();
    }

    private void HandleDisconnection() {
      if (manualDisconnect) {
        this.GetLogger().Log("Disconnected from {0}:{1}", Host, Port);
      } else {
        this.GetLogger().LogError("Disconnected from {0}:{1}", Host, Port);
      }
      Connected = false;
      manualDisconnect = false;

      OnDisconnect?.Invoke();
    }
  }
}
