using BlockEngine.Framework.Configuration;

namespace BlockEngine
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Settings.Initialize();

            using GameClient client = new();
            
            client.Run();
        }
    }
}