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
using MonoTorrent.Client;
using MonoTorrent.Client.Encryption;
using MonoTorrent;


namespace GameificClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientEngine Engine { get; set; }
        List<TorrentManager> managers = new List<TorrentManager>();
        private User _loggedIn;
        private Game dlGame;
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

        private async void downloadButton_Click(object senderr, RoutedEventArgs ee)
        {
            Game selGame = (Game) gameListBox.SelectedItem;
            Networking.reqTorrent(selGame._remoteFileName, "127.0.0.1");
            dlGame = selGame;
            var downloadsPath = Path.Combine(Environment.CurrentDirectory, "Downloads");

            // .torrent files will be loaded from this directory (if any exist)
            var torrentsPath = Path.Combine(Environment.CurrentDirectory, "Games");

            // If the torrentsPath does not exist, we want to create it
            if (!Directory.Exists(torrentsPath))
                Directory.CreateDirectory(torrentsPath);


            // Give an example of how settings can be modified for the engine.
            var settingBuilder = new EngineSettingsBuilder
            {
                // Allow the engine to automatically forward ports using upnp/nat-pmp (if a compatible router is available)
                AllowPortForwarding = true,

                // Automatically save a cache of the DHT table when all torrents are stopped.
                AutoSaveLoadDhtCache = true,

                // Automatically save 'FastResume' data when TorrentManager.StopAsync is invoked, automatically load it
                // before hash checking the torrent. Fast Resume data will be loaded as part of 'engine.AddAsync' if
                // torrent metadata is available. Otherwise, if a magnetlink is used to download a torrent, fast resume
                // data will be loaded after the metadata has been downloaded. 
                AutoSaveLoadFastResume = true,

                // If a MagnetLink is used to download a torrent, the engine will try to load a copy of the metadata
                // it's cache directory. Otherwise the metadata will be downloaded and stored in the cache directory
                // so it can be reloaded later.
                AutoSaveLoadMagnetLinkMetadata = true,

                // Use a fixed port to accept incoming connections from other peers.
                ListenPort = 55123,

                // Use a random port for DHT communications.
                DhtPort = 55123,
            };
            Engine = new ClientEngine(settingBuilder.ToSettings());




            var torrent = await Torrent.LoadAsync(selGame._remoteFileName + ".torrent");

            // EngineSettings.AutoSaveLoadFastResume is enabled, so any cached fast resume
            // data will be implicitly loaded. If fast resume data is found, the 'hash check'
            // phase of starting a torrent can be skipped.
            // 
            // TorrentSettingsBuilder can be used to modify the settings for this
            // torrent.
            var managers = await Engine.AddAsync(torrent, downloadsPath);
            managers.PeersFound += Manager_PeersFound;
            Console.WriteLine(torrent.InfoHash.ToString());

            foreach (TorrentManager manager in Engine.Torrents)
            {
                manager.PeerConnected += (o, e) =>
                {

                    Console.WriteLine($"Connection succeeded: {e.Peer.Uri}");
                };
                manager.ConnectionAttemptFailed += (o, e) =>
                {

                    Console.WriteLine(
                        $"Connection failed: {e.Peer.ConnectionUri} - {e.Reason}");
                };
                // Every time a piece is hashed, this is fired.
                manager.PieceHashed += delegate (object o, PieceHashedEventArgs e)
                {
                    Console.WriteLine($"Piece Hashed: {e.PieceIndex} - {(e.HashPassed ? "Pass" : "Fail")}");
                    this.Dispatcher.Invoke(() =>
                    {
                        progressBar.Value = e.TorrentManager.Bitfield.PercentComplete;
                    });
                };

                // Every time the state changes (Stopped -> Seeding -> Downloading -> Hashing) this is fired
                manager.TorrentStateChanged += Manager_TorrentStateChanged;

                // Every time the tracker's state changes, this is fired
                manager.TrackerManager.AnnounceComplete += (sender, e) =>
                {
                    Console.WriteLine($"{e.Successful}: {e.Tracker}");
                };


                // Start the torrentmanager. The file will then hash (if required) and begin downloading/seeding.
                // As EngineSettings.AutoSaveLoadDhtCache is enabled, any cached data will be loaded into the
                // Dht engine when the first torrent is started, enabling it to bootstrap more rapidly.
                await manager.StartAsync();
            }

        }


        private void Manager_PeersFound(object sender, PeersAddedEventArgs e)
        {
            Console.WriteLine($"Found {e.NewPeers} new peers and {e.ExistingPeers} existing peers");
        }

        private void Manager_TorrentStateChanged(object sender, TorrentStateChangedEventArgs e)
        {
            if (e.NewState == TorrentState.Seeding)
            {
                e.TorrentManager.StopAsync();
                this.Dispatcher.Invoke(() =>
                {
                    textBlockConsole.Text = "Extracting...";
                    progressBar.IsIndeterminate = true;
                });
                if(zipExtract(Path.Combine(Environment.CurrentDirectory, "Downloads") + "\\" + dlGame._remoteFileName))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        Game temp = (Game)gameListBox.SelectedItem;
                        temp = dlGame;
                        Helpers.WriteToJsonFile("Game.json", (List<Game>)gameListBox.ItemsSource, false);
                        textBlockConsole.Text = "Complete!";
                        progressBar.IsIndeterminate = false;
                        downloadButton.IsEnabled = false;
                        launchButton.IsEnabled = true;
                        buttonConfig.IsEnabled = true;
                    });
                }
                else
                {
                    textBlockConsole.Text = "Extraction Failed! Please try removing the folder in Games, and clearing Downloads, then re-downloading the title.";
                    progressBar.IsIndeterminate = false;
                }
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

        private bool zipExtract(string zip)
        {
            try
            {
                ZipFile.ExtractToDirectory(zip, "Games/" + dlGame._gameName);
                dlGame._isInstalled = true;
                dlGame._gameLocation = Environment.CurrentDirectory + "/Games/" + dlGame._gameName;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void launchButton_Click(object sender, RoutedEventArgs e)
        {
            Game temp = (Game)gameListBox.SelectedItem;
            Process process = new Process();
            // Configure the process using the StartInfo properties.
            process.StartInfo.FileName = temp._gameLocation + temp.relPathToExe;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            process.Start();
            process.WaitForExit();
        }
    }
}
