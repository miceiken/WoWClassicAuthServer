using System;
using System.Net;

namespace WoWClassicServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var srv = new AuthServer.AuthServer();
            srv.Listen(new IPEndPoint(IPAddress.Any, 3724));
            Console.WriteLine("...");
            Console.ReadKey();
        }
    }
}
