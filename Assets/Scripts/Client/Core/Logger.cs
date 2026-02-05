namespace Client {
  public class Logger : LoggerBase {

    public static Logger ForObject(UnityEngine.Object context) {
      return new Logger(context);
    }

    private Logger(UnityEngine.Object context) : base("Client", context) { }
  }

  public static class UnityEngineObjectLoggerExtensions {
    public static Logger GetLogger(this UnityEngine.Object context) {
      return Logger.ForObject(context);
    }
  }
}
