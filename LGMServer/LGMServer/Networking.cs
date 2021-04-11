using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MonoTorrent;
using MonoTorrent.Client;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace LGMServer
{
    public class Networking
    {
        private static int _port = 28000;
        private static int _cliPort = 27000;

        private static string _multicastGroupAddress = "239.1.1.1";

        private static UdpClient _sender;
        private static UdpClient _receiver;

        private static Thread _receiveThread;

        public static void initNetwork()
        {
            _receiver = new UdpClient();
            _receiver.JoinMulticastGroup(IPAddress.Parse(_multicastGroupAddress));
            _receiver.Client.Bind(new IPEndPoint(IPAddress.Any, _cliPort));
            _sender = new UdpClient();
            _sender.JoinMulticastGroup(IPAddress.Parse(_multicastGroupAddress));

            _receiveThread = new Thread(() =>
            {
                while (true)
                {
                    IPEndPoint sentBy = new IPEndPoint(IPAddress.Any, _port);
                    var dataGram = _receiver.Receive(ref sentBy);
                    string sendData;
                    byte[] sendDataBytes;

                    switch (Encoding.UTF8.GetString(dataGram))
                    {
                        case string a when a.Contains("GameificClientLogin"):
                            string login = a.Replace("GameificClientLogin", "");
                            string[] creds = login.Split('|');
                            string loginToken = "";
                            if (creds.Length == 3)
                            { 
                                loginToken = loginDb(creds[0], creds[1], creds[2]);
                            }
                            else
                            {
                                loginToken = loginDb(creds[0], creds[1]);
                            }

                            if (loginToken == "INVALID")
                            {
                                Console.WriteLine("Invalid login");
                            }
                            else if (loginToken == "CONNFAILED")
                            {
                                Console.WriteLine("Connection to User DB failed..");
                            }
                            else
                            {
                                //TODO: CHANGE THIS
                                User user = new User();
                                user._username = creds[0];
                                user._token = loginToken;
                                user._uid = 1;
                                user._accType = 1;
                                byte[] userBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(user));
                                _sender.Send(userBytes, userBytes.Length, sentBy);

                            }
                            break;
                        case "GameificClientReqGames":
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Sending " + sentBy.Address + " Game List...");
                            sendData = File.ReadAllText("Game.json");
                            sendDataBytes = Encoding.UTF8.GetBytes(sendData);
                            _sender.Send(sendDataBytes, sendDataBytes.Length, sentBy);
                            break;
                        case "GameificClientPowerOn":
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Client " + sentBy.Address + " has connected...");
                            sendData = "200 OK";
                            sendDataBytes = Encoding.UTF8.GetBytes(sendData);
                            _sender.Send(sendDataBytes, sendDataBytes.Length, sentBy);
                            break;
                        case string a when a.Contains("DlReq"):
                            Console.ForegroundColor = ConsoleColor.Green;
                            string file = Path.GetFileNameWithoutExtension(a).Replace("DlReq","");
                            Console.WriteLine($"Client {sentBy.Address} has requested {file}");
                            sendDataBytes = File.ReadAllBytes($".\\Torrents\\{file}.torrent");
                            _sender.Send(sendDataBytes, sendDataBytes.Length, sentBy);
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("New Client: " + sentBy.Address + " Info: " +
                                              Encoding.UTF8.GetString(dataGram));
                            var data = Encoding.UTF8.GetBytes("LGMServer" +
                                                              Assembly.GetEntryAssembly().GetName().Version.ToString());
                            _sender.Send(data, data.Length, sentBy);
                            break;
                    }
                }
            });
            _receiveThread.IsBackground = true;
            _receiveThread.Start();

            bool isDone = false;
            while (!isDone)
            {

            }
        }

        //For true offline capability, LGMServer runs it's own Torrent tracker.
        public static void startTracker()
        {
            Thread trackerThread = new Thread(() =>
            {
                new MySimpleTracker();
            });
            trackerThread.Start();
        }

        public static async void seedTorrents()
        {
            //ebin system

            string[] tempFS = Directory.GetFiles(".\\Torrents");
            List<string> torrentList = new List<string>();
            foreach (string s in tempFS)
            {
                if (s.Contains(".torrent"))
                {
                    torrentList.Add(s);
                }
            }

            int tCount = torrentList.Count;
            for (int i = 0; i < tCount; i++)
            {
                string temp = torrentList[i];
                ThreadPool.QueueUserWorkItem(_ => seedATorrent(temp));
            }
        }

        static void seedATorrent(string torrentFile)
        {
            Console.WriteLine("Initialising seed for " + torrentFile);
            ClientEngine engine = new ClientEngine(new EngineSettings());
            Torrent torrent = Torrent.Load(torrentFile);

            Debug.WriteLine("Checking for file to seed...");
            if (File.Exists(".\\TorrentData\\" + torrent.Files[0].Path))
            {
                BitField bitfield = new BitField(torrent.Pieces.Count).Not();
                FastResume fastResumeData = new FastResume(torrent.InfoHash, bitfield);

                TorrentManager manager = new TorrentManager(torrent, ".\\TorrentData", new TorrentSettings());
                manager.LoadFastResume(fastResumeData);

                engine.Register(manager);
                engine.StartAll();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + torrentFile + " does NOT have a matching file in TorrentData. We can't seed this one!");
                return;
            }
        }

        static string loginDb(string user, string hashpw, string token = "")
        {
            var dbCon = DBConnection.Instance();
            dbCon.Server = "nzgamer41.win";
            dbCon.DatabaseName = "gameific";
            dbCon.UserName = "gameificserver";
            dbCon.Password = "iRgBDEfVbTDI4ocs";
            if (dbCon.IsConnect())
            {
                //suppose col0 and col1 are defined as VARCHAR in the DB
                string query = "SELECT username,pass FROM gameific WHERE username='" + user +"'";
                var cmd = new MySqlCommand(query, dbCon.Connection);
                var reader = cmd.ExecuteReader();
                bool isCorrect = false;
                while (reader.Read())
                {
                    if (reader.GetString(1) == hashpw)
                    {
                        isCorrect = true;
                    }
                    else if (token != "" && reader.GetString(2) == token && reader.GetDateTime(3).CompareTo(DateTime.Now) > 0)
                    {
                        isCorrect = true;
                    }
                }

                if (isCorrect && token != "")
                {
                    reader.Close();
                    return token;
                }
                else if (isCorrect)
                {
                    var allChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var random = new Random();
                    var resultToken = new string(
                        Enumerable.Repeat(allChar, 8)
                            .Select(tokenn => tokenn[random.Next(tokenn.Length)]).ToArray());

                    string authToken = resultToken.ToString();
                    DateTime expiry = DateTime.Now.AddDays(30);
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
                    string updateQuery = "UPDATE gameific SET token='" + authToken + "', expiry='" +
                                         expiry.ToShortDateString() + "' WHERE username='" + user +"'";
                    reader.Close();
                    var cmd2 = new MySqlCommand(updateQuery, dbCon.Connection);
                    cmd2.ExecuteNonQuery();
                    return authToken;
                }
                else
                {
                    return "INVALID";
                }
            }
            else
            {
                return "CONNFAILED";
            }
        }
        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
