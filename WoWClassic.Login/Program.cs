using System;
using System.Net;
using WoWClassic.Common.Log;

namespace WoWClassic.Login
{
    class Program
    {
        private static IPAddress BindAddress = IPAddress.Any;
        private static int BindPort = 3724;

        private static void Main(string[] args)
        {
            Console.Title = "WoWAuthServer";

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-ip":
                    case "--ip": // Defaults to IPAdress.Any (0.0.0.0)
                        string argIP = null;
                        if (!TryReadArg(args, ref i, out argIP) || !IPAddress.TryParse(argIP, out BindAddress))
                        {
                            Console.WriteLine("Unable to parse IP...");
                            Console.ReadKey();
                            return;
                        }
                        break;

                    case "-p":
                    case "--port": // Defaults to 3724 (Retail)
                        string argPort = null;
                        if (!TryReadArg(args, ref i, out argPort) || !int.TryParse(argPort, out BindPort))
                        {
                            Console.WriteLine("Unable to parse port...");
                            Console.ReadKey();
                            return;
                        }
                        break;
                }
            }

            Log.CreateLogger<AuthLogTypes>();

            Log.AddSubscriber(AuthLogTypes.Packets, new FileLogSubscriber("Logs/LoginPackets.txt"));
            Log.AddSubscriber(AuthLogTypes.Packets, new ConsoleLogSubscriber("[PACKET] "));

            Log.AddSubscriber(AuthLogTypes.Debug, new FileLogSubscriber("Logs/LOGINDEBUG.txt"));
            Log.AddSubscriber(AuthLogTypes.Debug, new ConsoleLogSubscriber("[DEBUG] "));

            Log.AddSubscriber(AuthLogTypes.General, new FileLogSubscriber("Logs/LoginServer.txt", "GENERAL"));
            Log.AddSubscriber(AuthLogTypes.General, new ConsoleLogSubscriber());

            var ep = new IPEndPoint(BindAddress, BindPort);
            AuthServer.Instance.Listen(ep);
            Log.WriteLine(AuthLogTypes.General, "Server is now listening at {0}:{1}", ep.Address, ep.Port);

            Console.ReadKey();
        }

        private static bool TryReadArg(string[] args, ref int i, out string arg)
        {
            arg = null;
            if (i >= args.Length - 1)
                return false;
            arg = args[++i];
            return true;
        }
    }

    public enum AuthLogTypes
    {
        General,
        Packets,
        Debug
    }
}
