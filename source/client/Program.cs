using BlockEngine.Client.Framework.Configuration;

namespace BlockEngine.Client
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Settings.Initialize();

            using GameClient client = new(args);
            
            client.Run();
        }
    }
}