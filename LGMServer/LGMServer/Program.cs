using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LGMServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("LGMServer v" + Assembly.GetEntryAssembly().GetName().Version.ToString());
            Networking.startTracker();
            Networking.seedTorrents();
            Networking.initNetwork();
            
            Console.WriteLine("Server stopped");
            Console.ReadLine();
        }
    }
}
