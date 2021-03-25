using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameificClient
{
    [Serializable]
    public class AppSettings
    {
        public string serverIP { get; set; }
        public bool offlineMode { get; set; } = false;
        public bool p2pBeta { get; set; } = false;

        public AppSettings()
        {

        }

    }
}
