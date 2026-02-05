namespace Server {
  public class Logger : LoggerBase {

    public static Logger ForObject(UnityEngine.Object context, bool enabled = true) {
      Logger logger = new(context) { Enabled = enabled };
      return logger;
    }

    private Logger(UnityEngine.Object context) : base("Server", context) { }
  }

  public static class UnityEngineObjectLoggerExtensions {
    public static Logger GetLogger(this UnityEngine.Object context, bool enabled = true) {
      Logger logger = Logger.ForObject(context);
      logger.Enabled = enabled;
      return logger;
    }
  }
}
