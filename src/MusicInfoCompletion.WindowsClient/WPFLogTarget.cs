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
            if (OnPage1)
            {
                LogMessageToPage1?.Invoke(Layout.Render(logEvent));
            }
            else
            {
                LogMessageToPage2?.Invoke(Layout.Render(logEvent));
            }
        }

        public static bool OnPage1 { get; set; } = true;

        public static Action<string> LogMessageToPage1 { get; set; }

        public static Action<string> LogMessageToPage2 { get; set; }

        public static Action ClearLogForPage1 { get; set; }

        public static Action ClearLogForPage2 { get; set; }
    }

    public static class LoggerHelper
    {
        public static Logger Logger { get; } = LogManager.GetCurrentClassLogger();
    }
}
