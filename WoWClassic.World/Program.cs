using System;
using System.Net;
using WoWClassic.Common.Log;

namespace WoWClassic.World
{
    class Program
    {
        private static IPAddress BindAddress = IPAddress.Any;
        private static int BindPort = 8085;

        private static void Main(string[] args)
        {
            Console.Title = "WoWWorldServer";

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

            Log.CreateLogger<WorldLogTypes>();
            Log.AddSubscriber(WorldLogTypes.General, new ConsoleLogSubscriber("[GENERAL] "));
            Log.AddSubscriber(WorldLogTypes.Packets, new FileLogSubscriber("Logs/WorldPackets.txt"));
            Log.AddSubscriber(WorldLogTypes.Packets, new ConsoleLogSubscriber("[PACKET] "));

            WorldManager.Instance.Initialize();
            // TODO: config/db stuff for realms
            WorldManager.Instance.Service.RealmID = 1;

            Console.ReadKey();
        }
        // https://github.com/RomanRom2/WoWCore/blob/03eb99d0a84606de44413b4fae123f1b89b5dac6/05875_1.12.1/pas/sandbox/ClassConnection.pas#L560-L730
        private static bool TryReadArg(string[] args, ref int i, out string arg)
        {
            arg = null;
            if (i >= args.Length - 1)
                return false;
            arg = args[++i];
            return true;
        }
    }

    public enum WorldLogTypes
    {
        General,
        Debug,
        Packets,
        DataStructure,
    }
}
