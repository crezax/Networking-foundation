using System.Collections.Generic;
using UnityEngine;

public class LogHandler : ILogHandler {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void SetupLogHandler() {
    UnityHandler = Debug.unityLogger.logHandler;
    Debug.unityLogger.logHandler = new LogHandler();
  }

  public readonly struct Entry {
    public readonly System.DateTime time;
    public readonly LogType type;
    public readonly Object context;
    public readonly string format;
    public readonly object[] args;

    public Entry(LogType type, Object context, string format, params object[] args) {
      this.time = System.DateTime.Now;
      this.type = type;
      this.context = context;
      this.format = format;
      this.args = args;
    }
  }

  public delegate void NewLogEntry(Entry logEntry);
  public static NewLogEntry OnNewLogEntry;

  private static List<Entry> logEntries = new List<Entry>();
  public static List<Entry> LogEntries => new List<Entry>(logEntries);
  public static ILogHandler UnityHandler { get; private set; }

  private LogHandler() { }

  public void LogFormat(LogType logType, Object context, string format, params object[] args) {
    UnityHandler.LogFormat(logType, context, format, args);
    AddLogEntry(new Entry(logType, context, format, args));
  }

  public void LogException(System.Exception exception, Object context) {
    UnityHandler.LogException(exception, context);
    AddLogEntry(new Entry(LogType.Error, context, "{0}", exception.Message));
    foreach (string stackTraceElement in exception.StackTrace.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.None)) {
      AddLogEntry(new Entry(LogType.Error, context, "{0}", stackTraceElement));
    }
  }

  private void AddLogEntry(Entry entry) {
    logEntries.Add(entry);
    if (logEntries.Count > 10000) {
      logEntries.RemoveAt(0);
    }
    OnNewLogEntry?.Invoke(entry);
  }
}
