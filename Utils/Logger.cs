namespace BlockEngine.Utils
{
    public static class Logger
    {
        private const string LOG_PREFIX = $"[{Constants.ENGINE_NAME}]";
        
        public static bool EnableVerboseLogging = false;
        
        
        public static void LogVerbose(string message)
        {
            if(EnableVerboseLogging)
                WriteLine(message);
        }
        
        
        public static void Log(string message)
        {
            WriteLine(message);
        }
        
        
        public static void LogWarning(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

        
        public static void LogError(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine(message);
            Console.ForegroundColor = originalColor;
        }


        private static void WriteLine(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"{LOG_PREFIX} [{time}]: {message}");
        }
    }
}