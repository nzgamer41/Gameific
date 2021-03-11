using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANGameManager
{
    [Serializable]
    public class Game
    {
        public string _gameName { get; set; }
        public bool _isInstalled { get; set; }
        public string _gameLocation { get; set; }
        public string _gameArgs { get; set; }
        public bool _needsSteamEmu { get; set; }
        public string _steamEmuProfile { get; set; }
        public string _remoteFileName { get; set; }
        public bool _initSetupComplete { get; set; }

        public Game()
        {

        }

        public override string ToString()
        {
            return _gameName;
        }
    }
}
