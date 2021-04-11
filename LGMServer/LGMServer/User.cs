using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LGMServer
{
    [Serializable]
    public class User
    {
        public string _username;
        public string _token;
        public DateTime _tokenExp;
        public int _uid;
        public int _accType;

        public User()
        {
        }
    }
}
