using UnityEngine;
using Object = UnityEngine.Object;

namespace Preliy.Flange
{
    public static class Logger
    {
        public const string TAG_PACKAGE = "<b><color=#737cff>Flange</color></b>";
        public readonly static ILogger UnityLogger = Debug.unityLogger;

        public static void Log(LogType logType, object message)
        {
            UnityLogger.Log(logType, TAG_PACKAGE, message);
        }
        
        public static void Log(LogType logType, object message, Object context)
        {
            UnityLogger.Log(logType, TAG_PACKAGE, message, context);
        }
    }
}
