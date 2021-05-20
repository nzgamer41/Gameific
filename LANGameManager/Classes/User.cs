using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameificClient
{
    [Serializable]
    public class User
    {
        public string _username = "offline";
        public string _token;
        private DateTime _tokenExp;
        private int _uid;
        private int _accType;

        public User(string username = "", string password = "")
        {
            if (username != "")
            {
                _username = username;
            }
            if (password != "")
            {
                _token = password;
            }
        }

        public bool logon(string username, string pw)
        {
            try
            {
                if (_token != "" && _tokenExp.CompareTo(DateTime.Now) >= 0)
                {
                    return true;
                }
                else
                {
                    User temp = Networking.logOn(username, pw, "127.0.0.1");
                    _username = temp._username;
                    _token = temp._token;
                    _tokenExp = temp._tokenExp;
                    _uid = temp._uid;
                    _accType = temp._accType;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
