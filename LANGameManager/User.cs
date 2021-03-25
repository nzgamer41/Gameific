using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace GameificClient
{
    class User
    {
        private string _username;
        private string _token;
        private DateTime _tokenExp;
        private int _uid;
        private int _accType;

        public User(string username, string pw)
        {
            //Connect to Server
            //Send Login info
            //wait for token response
            //Store token, mark login as successful.
        }
    }
}
