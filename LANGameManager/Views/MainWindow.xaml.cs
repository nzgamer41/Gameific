using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Net;
using System.Threading;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;

using System.Net;
using OctoTorrent.Client;
using OctoTorrent.Client.Encryption;
using OctoTorrent.Common;


namespace GameificClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BanList banlist;
        ClientEngine engine;
        List<TorrentManager> managers = new List<TorrentManager>();
        private User _loggedIn;
        public MainWindow(User loggedInUser)
        {
            InitializeComponent();
            if (loggedInUser._username != "offline")
            {
                Networking.requestGamesList("127.0.0.1");
            }

            if (File.Exists("Game.json"))
            {
                List<Game> gamelist = Helpers.ReadFromJsonFile<List<Game>>("Game.json");
                gameListBox.ItemsSource = gamelist;
            }
            downloadButton.IsEnabled = false;
            launchButton.IsEnabled = false;
            buttonConfig.IsEnabled = false;
            _loggedIn = loggedInUser;
            this.Title += " logged in as " + _loggedIn._username;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow cw = new ConfigWindow((Game)gameListBox.SelectedItem);
            cw.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void gameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Game selGame = (Game)gameListBox.SelectedItem;
            if (selGame._isInstalled)
            {
                downloadButton.IsEnabled = false;
                launchButton.IsEnabled = true;
                buttonConfig.IsEnabled = true;
            }
            else
            {
                downloadButton.IsEnabled = true;
                launchButton.IsEnabled = false;
                buttonConfig.IsEnabled = false;
            }
        }

        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            Game selGame = (Game) gameListBox.SelectedItem;
            Networking.reqTorrent(selGame._remoteFileName, "127.0.0.1");

            EngineSettings settings = new EngineSettings();
            settings.AllowedEncryption = ChooseEncryption();

            // If both encrypted and unencrypted connections are supported, an encrypted connection will be attempted
            // first if this is true. Otherwise an unencrypted connection will be attempted first.
            settings.PreferEncryption = true;

            // Torrents will be downloaded here by default when they are registered with the engine
            settings.SavePath = ".\\TorrentData";

            // The maximum upload speed is 200 kilobytes per second, or 204,800 bytes per second
            settings.GlobalMaxUploadSpeed = 200 * 1024;

            engine = new ClientEngine(settings);

            // Tell the engine to listen at port 6969 for incoming connections
            engine.ChangeListenEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6970));

            // Load a .torrent file into memory
            Torrent torrent = Torrent.Load(selGame._remoteFileName + ".torrent");


            TorrentManager manager = new TorrentManager(torrent, "TorrentData", new TorrentSettings());
            managers.Add(manager);
            engine.Register(manager);

            // Disable rarest first and randomised picking - only allow priority based picking (i.e. selective downloading)
            PiecePicker picker = new StandardPicker();
            picker = new PriorityPicker(picker);
            manager.ChangePicker(picker);
            manager.PieceHashed += Manager_PieceHashed;
            manager.TorrentStateChanged += Manager_TorrentStateChanged;
            engine.StartAll();
            
        }

        private void Manager_TorrentStateChanged(object sender, TorrentStateChangedEventArgs e)
        {
            if (e.NewState == TorrentState.Seeding)
            {
                e.TorrentManager.Stop();
                //extract
            }
            else if (e.NewState == TorrentState.Downloading)
            {
                this.Dispatcher.Invoke(() =>
                {
                    textBlockConsole.Text = "Downloading game...";
                });
            }
            else
            {
                Console.WriteLine(e.NewState.ToString());
            }
        }

        private void Manager_PieceHashed(object sender, PieceHashedEventArgs e)
        {
            var progress = (float) e.PieceIndex / e.TorrentManager.Bitfield.Length;
            this.Dispatcher.Invoke(() =>
            {
                progressBar.Value = progress;
            });

        }

        EncryptionTypes ChooseEncryption()
        {
            EncryptionTypes encryption;
            // This completely disables connections - encrypted connections are not allowed
            // and unencrypted connections are not allowed
            encryption = EncryptionTypes.None;

            // Only unencrypted connections are allowed
            encryption = EncryptionTypes.PlainText;

            // Allow only encrypted connections
            encryption = EncryptionTypes.RC4Full | EncryptionTypes.RC4Header;

            // Allow unencrypted and encrypted connections
            encryption = EncryptionTypes.All;
            encryption = EncryptionTypes.PlainText | EncryptionTypes.RC4Full | EncryptionTypes.RC4Header;

            return encryption;
        }
    }
}
