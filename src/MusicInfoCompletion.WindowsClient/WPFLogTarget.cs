using System;
using NLog;
using NLog.Common;
using NLog.Targets;

namespace MusicInfoCompletion.WindowsClient
{
    [Target("WPFLogTarget")]
    public class WPFLogTarget : TargetWithLayout
    {
        protected override void Write(LogEventInfo logEvent)
        {
            LogMessage?.Invoke(Layout.Render(logEvent));
        }

        public static Action<string> LogMessage { get; set; }

        public static Action ClearLog { get; set; }
    }

    public static class LoggerHelper
    {
        public static Logger Logger { get; } = LogManager.GetCurrentClassLogger();
    }
}
