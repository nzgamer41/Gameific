using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using Newtonsoft.Json;

namespace GameificClient
{
    public class Networking
    {
        private static int _port = 27000;
        private static int _srvPort = 28000;

        private static UdpClient _receiver;

        public static void requestGamesList(string serverIP)
        {
            _receiver = new UdpClient();

            IPEndPoint srvEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), _port);
            var data = Encoding.UTF8.GetBytes("LGMClientReqGames");
            _receiver.Send(data, data.Length, srvEndPoint);

            IPEndPoint sentBy = new IPEndPoint(IPAddress.Any, _srvPort);
            var dataGram = _receiver.Receive(ref srvEndPoint);
            string recvData = Encoding.UTF8.GetString(dataGram);
            List<Game> games = JsonConvert.DeserializeObject<List<Game>>(recvData);
            Helpers.WriteToJsonFile("Game.json", games, false);
        }

        /// <summary>
        /// This function is an internal function called by the User class.
        /// </summary>
        /// <returns></returns>
        public static User logOn(string username, string pw, string serverIP)
        {
            //security commands FIRST
            byte[] salt1 = Encoding.ASCII.GetBytes("VOMVF2YR5FTORZZOGD0Y");
            Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(pw, salt1, 1000);

            Aes encAlg = Aes.Create();
            encAlg.Key = k1.GetBytes(16);
            MemoryStream encStream = new MemoryStream();
            CryptoStream enc = new CryptoStream(encStream, encAlg.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] data1 = new UTF8Encoding(false).GetBytes(pw);
            enc.Write(data1, 0, data1.Length);
            enc.FlushFinalBlock();
            enc.Close();

            _receiver = new UdpClient();
            IPEndPoint srvEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), _port);
            var data = Encoding.UTF8.GetBytes("GameificClientLogin");
            byte[] rv = new byte[data.Length + encStream.ToArray().Length];
            System.Buffer.BlockCopy(data, 0, rv, 0, data.Length);
            Buffer.BlockCopy(encStream.ToArray(), 0, rv, data.Length, encStream.ToArray().Length);
            k1.Reset();
            encStream.Dispose();
            _receiver.Send(rv, rv.Length, srvEndPoint);

            var dataGram = _receiver.Receive(ref srvEndPoint);
            return (User)Helpers.ByteArrayToObject(dataGram);
        }

        public static bool powerOn(string serverIP)
        {
            try
            {
                _receiver = new UdpClient();

                IPEndPoint srvEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), _port);
                var data = Encoding.UTF8.GetBytes("GameificClientPowerOn");
                _receiver.Send(data, data.Length, srvEndPoint);

                IPEndPoint sentBy = new IPEndPoint(IPAddress.Any, _srvPort);
                var dataGram = _receiver.Receive(ref srvEndPoint);
                string recvData = Encoding.UTF8.GetString(dataGram);
                if (recvData == "200 OK")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (SocketException ex)
            {
                return false;
            }
        }

        public static bool reqTorrent(string remoteFileName, string serverIP)
        {
            try
            {
                _receiver = new UdpClient();

                IPEndPoint srvEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), _port);
                var data = Encoding.UTF8.GetBytes("DlReq"+remoteFileName);
                _receiver.Send(data, data.Length, srvEndPoint);

                IPEndPoint sentBy = new IPEndPoint(IPAddress.Any, _srvPort);
                var dataGram = _receiver.Receive(ref srvEndPoint);
                File.WriteAllBytes(".\\"+remoteFileName+".torrent", dataGram);
                return true;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }
    }
}
