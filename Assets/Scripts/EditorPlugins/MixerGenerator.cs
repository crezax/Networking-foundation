#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class MixerGenerator : AssetPostprocessor {
  enum MixerType { CLIENT, SERVER };

  static bool anyChanges = false;

  static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
    anyChanges = false;

    foreach (string str in importedAssets) {
      anyChanges = CoreFlatBuffersAssetPath(str) || anyChanges;

      if (anyChanges) {
        break;
      }
    }

    if (anyChanges) {
      AssetDatabase.Refresh();
    }
  }

  private static bool CoreFlatBuffersAssetPath(string assetPath) {
    if (!assetPath.EndsWith("Transport/Core/messages.fbs")) {
      return false;
    }

    string fbsFileSystemPath = Directory.GetParent(Application.dataPath) + Path.DirectorySeparatorChar.ToString() + assetPath;
    StreamReader reader = new(fbsFileSystemPath);

    List<string> clientMessages = new();
    List<string> serverMessages = new();
    bool clientMessage = false;
    bool serverMessage = false;

    for (string line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
      if (line.Trim().StartsWith("union ClientMessageContent")) {
        clientMessage = true;
      } else if (line.Trim().StartsWith("union ServerMessageContent")) {
        serverMessage = true;
      } else if (line.Trim().StartsWith("}")) {
        clientMessage = false;
        serverMessage = false;
      } else if (clientMessage) {
        clientMessages.Add(line.Trim(' ', ','));
      } else if (serverMessage) {
        serverMessages.Add(line.Trim(' ', ','));
      }
    }

    reader.Close();

    string clientMixer = GetMixer(MixerType.CLIENT, serverMessages);
    string serverMixer = GetMixer(MixerType.SERVER, clientMessages);

    using (StreamWriter file =
                new(Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "Assets", "Scripts", "Client", "Core", "Mixer.cs"))) {
      file.Write(clientMixer);
    }
    using (StreamWriter file =
                new(Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "Assets", "Scripts", "Server", "Core", "Mixer.cs"))) {
      file.Write(serverMixer);
    }

    return true;
  }

  private static string GetMixer(MixerType type, List<string> messages) {
    string mixer = string.Format(@"// This file is auto generated. Do NOT edit manually. Edit MixerGenerator.cs instead.
using UnityEngine;
using Transport.Messages;
using System;
using System.Collections.Generic;

namespace {0} {{
  public class Mixer : MonoBehaviour {{", type == MixerType.CLIENT ? "Client" : "Server");

    foreach (string message in messages) {
      mixer += string.Format(@"
    private readonly Dictionary<long, {0}Delegate> {1}Callbacks = new();
    public delegate void {0}Delegate({0} message);
    ", message, char.ToLower(message[0]) + message.Substring(1));
      if (type == MixerType.SERVER) {
        mixer += string.Format(@"
    private readonly Dictionary<long, {0}DelegateWithConnectionId> {1}CallbacksWithConnectionId = new();
    public delegate void {0}DelegateWithConnectionId(int connectionId, {0} message);
", message, char.ToLower(message[0]) + message.Substring(1));
      }
    }

    mixer += string.Format(@"
    private static Mixer instance;

    public static Mixer Instance {{
      get {{
        return instance;
      }}
    }}

    protected void Awake() {{
      if (instance != null) {{
        Destroy(gameObject);
        return;
      }}

      instance = this;
    }}

    protected void Start() {{
      AbstractNetworkClient.Instance.On{0}Message += On{0}Message;
    }}
", type == MixerType.CLIENT ? "Server" : "Client");

    foreach (string message in messages) {
      mixer += string.Format(@"    
    public void RegisterCallback(long objectId, {0}Delegate callback) {{
      {1}Callbacks[objectId] = Delegate.Combine({1}Callbacks.GetValueOrDefault(objectId, null), callback) as {0}Delegate;
    }}", message, char.ToLower(message[0]) + message.Substring(1));
      if (type == MixerType.SERVER) {
        mixer += string.Format(@"
    public void RegisterCallback(long objectId, {0}DelegateWithConnectionId callback) {{
      {1}CallbacksWithConnectionId[objectId] = Delegate.Combine({1}CallbacksWithConnectionId.GetValueOrDefault(objectId, null), callback) as {0}DelegateWithConnectionId;
    }}
",
        message,
        char.ToLower(message[0]) + message.Substring(1));
      }
    }

    foreach (string message in messages) {
      mixer += string.Format(@"    
    public void UnregisterCallback(long objectId, {0}Delegate callback) {{
      if ({1}Callbacks.ContainsKey(objectId)) {{
        {1}Callbacks[objectId] = Delegate.Remove({1}Callbacks[objectId], callback) as {0}Delegate;
      }}
    }}", message, char.ToLower(message[0]) + message.Substring(1));
      if (type == MixerType.SERVER) {
        mixer += string.Format(@"

    public void UnregisterCallback(long objectId, {0}DelegateWithConnectionId callback) {{
      if ({1}CallbacksWithConnectionId.ContainsKey(objectId)) {{
        {1}CallbacksWithConnectionId[objectId] = Delegate.Remove({1}CallbacksWithConnectionId[objectId], callback) as {0}DelegateWithConnectionId;
      }}
    }}
",
        message,
        char.ToLower(message[0]) + message.Substring(1));
      }
    }

    if (type == MixerType.CLIENT) {
      mixer += @"
    private void OnServerMessage(ServerMessage message) {
      switch (message.MessageType) {";
    } else {
      mixer += @"
    private void OnClientMessage(int connectionId, ClientMessage message) {
      GameObject obj;
      
      if (!NetworkedBehaviour.Index.TryGetValue(message.ServerId, out obj)) {
        this.GetLogger().LogWarning(""{0} sent message {1} while recipient ({2}) doesn't exist"", connectionId, message.MessageType, message.ServerId);
        return;
      }

      PlayerOwnedBehaviour playerOwnedBehaviour =
        obj.GetComponent<PlayerOwnedBehaviour>();
      Debug.Assert(
        obj.GetComponent<IAnyPlayerMessageReceiver>() != null || (playerOwnedBehaviour != null && playerOwnedBehaviour.CanReceiveMessages(
          PlayerRegistry.Instance.GetPlayerId(connectionId))),
        ""Player "" + connectionId + "" sent a message to "" + obj + "" despite not "" +
        ""having ownership. This might be caused by recent ownership change, "" +
        ""bug or a cheating attempt."");

      if (obj.GetComponent<IAnyPlayerMessageReceiver>() == null && (playerOwnedBehaviour == null || !playerOwnedBehaviour.CanReceiveMessages(
          PlayerRegistry.Instance.GetPlayerId(connectionId)))) {
        return;
      }
        
      switch (message.MessageType) {";
    }

    foreach (string message in messages) {
      mixer += string.Format(@"
        case {0}MessageContent.{1}:
          {1}Delegate {2}Delegate;
          if ({2}Callbacks.TryGetValue(message.ServerId, out {2}Delegate)) {{
            {2}Delegate?.Invoke(
              message.Message<{1}>().Value);
          }}", type == MixerType.CLIENT ? "Server" : "Client", message, char.ToLower(message[0]) + message.Substring(1));
      if (type == MixerType.SERVER) {
        mixer += string.Format(@"
          {0}DelegateWithConnectionId {1}DelegateWithConnectionId;
          if ({1}CallbacksWithConnectionId.TryGetValue(message.ServerId, out {1}DelegateWithConnectionId)) {{
            {1}DelegateWithConnectionId?.Invoke(
              connectionId,
              message.Message<{0}>().Value);
          }}",
        message,
        char.ToLower(message[0]) + message.Substring(1));
      }
      mixer += @"
          break;";
    }

    mixer += @"
      }
    }
  }
}";

    return mixer;
  }
}

#endif
