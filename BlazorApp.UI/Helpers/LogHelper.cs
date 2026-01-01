namespace BlazorApp.UI.Helpers
{
    public static class LogHelper
    {
        private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";

        public static void Log(string message)
        {
            var timestamp = DateTime.Now.ToString(TimestampFormat);
            Console.WriteLine($"[{timestamp}] {message}");
        }

        public static void LogError(string message)
        {
            var timestamp = DateTime.Now.ToString(TimestampFormat);
            Console.WriteLine($"[{timestamp}] ERROR: {message}");
        }

        public static string GetTimestampedMessage(string message)
        {
            var timestamp = DateTime.Now.ToString(TimestampFormat);
            return $"[{timestamp}] {message}";
        }
    }
}
