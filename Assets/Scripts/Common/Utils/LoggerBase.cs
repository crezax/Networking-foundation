using System;
using UnityEngine;

public abstract class LoggerBase {

  private readonly string tag;
  private readonly UnityEngine.Object context;

  public bool Enabled { get; set; }

  protected LoggerBase(string tag, UnityEngine.Object context) {
    this.tag = tag;
    this.context = context;
    Enabled = true;
  }

  public void Assert(bool condition, object obj) => Assert(condition, "{0}", obj);
  public void Assert(bool condition, string format, params object[] args) {
    Debug.Assert(condition, string.Format("{0}: {1}", tag, string.Format(format, args)), context);
  }

  public void Log(object obj) => Log("{0}", obj);
  public void Log(string format, params object[] args) {
    if (Enabled) {
      Debug.unityLogger.Log(tag, string.Format(format, args), context);
    }
  }

  public void LogDebug(object obj) => LogDebug("{0}", obj);
  public void LogDebug(string format, params object[] args) {
#if UNITY_EDITOR
    Log(format, args);
#endif
  }

  public void LogDebugWarning(object obj) => LogDebugWarning("{0}", obj);
  public void LogDebugWarning(string format, params object[] args) {
#if UNITY_EDITOR
    LogWarning(format, args);
#endif
  }

  public void LogError(object obj) => LogError("{0}", obj);
  public void LogError(string format, params object[] args) {
    if (Enabled) {
      Debug.unityLogger.LogError(tag, string.Format(format, args), context);
    }
  }

  public void LogException(Exception exception) {
    LogError("Exception: {0}", exception);
    Debug.unityLogger.LogException(exception, context);
  }

  public void LogWarning(object obj) => LogWarning("{0}", obj);
  public void LogWarning(string format, params object[] args) {
    if (Enabled) {
      Debug.unityLogger.LogWarning(tag, string.Format(format, args), context);
    }
  }
}
