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
using NAudio.Wave;
using Ookii.Dialogs.Wpf;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using LibZeroTier;
using Microsoft.Win32;
using AutoUpdaterDotNET;
using DiscordRPC;
using DiscordRPC.Logging;


namespace LANGameManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                initSettings();
                AutoUpdater.Start("http://" + _appSettings.serverIP + "/updates/LANGame.xml");
                Initialize();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.Close();
            }
        }
        private static AutoResetEvent TransferFinishedEvent = new AutoResetEvent(false);
        List<Game> basegameList = new List<Game>();
        public List<Game> gameList = new List<Game>();
        WaveOut waveOut = new WaveOut(); // or WaveOutEvent()
        Mp3FileReader reader;
        public static AppSettings _appSettings;

        public DiscordRpcClient client;

        private void initSettings()
        {
            if (File.Exists("LANGameManager.xml"))
            {
                _appSettings = Helpers.ReadFromXmlFile<AppSettings>("LANGameManager.xml");
            }
            else
            {
                MessageBox.Show("ERROR: You are missing LANGameManager.xml! This is critical to the program working!");
                throw new Exception("Missing LANGameManager.xml");
            }
        }




        //Called when your application first starts.
        //For example, just before your main loop, on OnEnable for unity.
        void Initialize()
        {
            /*
            Create a discord client
            NOTE: 	If you are using Unity3D, you must use the full constructor and define
                     the pipe connection.
            */
            client = new DiscordRpcClient("634935891218923561");

            //Set the logger
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            //Subscribe to events
            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };

            client.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine("Received Update! {0}", e.Presence);
            };

            //Connect to the RPC
            client.Initialize();

            //Set the rich presence
            //Call this as many times as you want and anywhere in your code.
#if DEBUG
            client.SetPresence(new RichPresence()
            {
                Details = "Debug Version " + Assembly.GetEntryAssembly().GetName().Version.ToString(),
                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    LargeImageText = "LAN Game Manager by nzgamer41",
                    SmallImageKey = "image_small"
                }
            });
#endif
#if (!DEBUG)
            client.SetPresence(new RichPresence()
            {
                Details = "Version " + Assembly.GetEntryAssembly().GetName().Version.ToString(),
                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    LargeImageText = "LAN Game Manager by nzgamer41",
                    SmallImageKey = "image_small"
                }
            });
#endif
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gameList[gameListBox.SelectedIndex]._isInstalled)
            {
                launchButton.IsEnabled = true;
            }
            else
            {
                launchButton.IsEnabled = false;
            }
            var bitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/question.png"));
            imageGame.Source = bitmap;
            textBlockConsole.Text = "Welcome to LAN Game Manager. The ZeroTier network you need to join is 83048a0632897230";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists("games.xml"))
            {
                basegameList = Helpers.ReadFromXmlFile<List<Game>>("games.xml");
                if (File.Exists("games.xml.init"))
                {
                    List<Game> newGameList = new List<Game>();
                    newGameList = Helpers.ReadFromXmlFile<List<Game>>("games.xml.init");
                    if (basegameList.Count < newGameList.Count)
                    {
                        for (int i = 0; i < newGameList.Count - basegameList.Count; i++)
                        {
                            basegameList.Add(newGameList[basegameList.Count + i]);
                        }
                    }
                    Helpers.WriteToXmlFile("games.xml", basegameList, false);
                    File.Delete("games.xml.init");
                }
            }
            else
            {
                File.Move("games.xml.init", "games.xml");
            }

            foreach (Game t in basegameList)
            {
                if (t._isInstalled)
                {
                    gameListBox.Items.Add(t);
                    gameList.Add(t);
                }
            }

            reader = new Mp3FileReader("assets\\menu.mp3");
            waveOut = new WaveOut(); // or WaveOutEvent()
            waveOut.Init(reader);
            waveOut.Play();
            textBlockConsole.Text = "Welcome to LAN Game Manager. The ZeroTier network you need to join is 83048a0632897230";
            if (gameListBox.Items.Count != 0)
            {
                gameListBox.SelectedIndex = 0;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Helpers.WriteToXmlFile("games.xml", basegameList, false);
            waveOut.Dispose();
            this.Close();
        }

        private bool regHelper()
        {
            //Red Alert 2 & UT2004 need some helper code to get running
            string regLocation = Path.GetDirectoryName(gameList[gameListBox.SelectedIndex]._gameLocation + "\\key.reg");
            Process regeditProcess = Process.Start("regedit.exe", "/s " + regLocation);
            regeditProcess.WaitForExit();
            return true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (gameListBox.Items.Count == 0)
            {
                MessageBox.Show("You have no games installed.");
                return;
            }
            waveOut.Stop();
            if (gameList[gameListBox.SelectedIndex]._isInstalled)
            {
                if (!gameList[gameListBox.SelectedIndex]._initSetupComplete)
                {
                    gameList[gameListBox.SelectedIndex]._initSetupComplete = regHelper();
                }
                ProcessStartInfo start = new ProcessStartInfo();
                if (gameList[gameListBox.SelectedIndex]._needsSteamEmu)
                {
                    if (File.Exists("sse\\SmartSteamLoader.exe"))
                    {
                        string text =
                            File.ReadAllText(Path.GetDirectoryName(gameList[gameListBox.SelectedIndex]._gameLocation) +
                                             "\\" + gameList[gameListBox.SelectedIndex]._steamEmuProfile);
                        text = text.Replace("%GAMEDIR%",
                            Path.GetDirectoryName(gameList[gameListBox.SelectedIndex]._gameLocation));
                        text = text.Replace("%SSE%", Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory) + "sse");
                        File.WriteAllText(
                            Path.GetDirectoryName(gameList[gameListBox.SelectedIndex]._gameLocation) + "\\" +
                            gameList[gameListBox.SelectedIndex]._steamEmuProfile, text);

                        // Enter in the command line arguments, everything you would enter after the executable name itself
                        start.Arguments = Path.GetDirectoryName(gameList[gameListBox.SelectedIndex]._gameLocation) +
                                          "\\" + gameList[gameListBox.SelectedIndex]._steamEmuProfile;
                        // Enter the executable to run, including the complete path
                        start.FileName = "sse\\SmartSteamLoader.exe";
                    }
                    else
                    {
                        MessageBox.Show(
                            "ALERT: The Steam loader appears to be missing from the launcher's directory.'");
                    }
                }
                else
                {
                    // Enter in the command line arguments, everything you would enter after the executable name itself
                    start.Arguments = gameList[gameListBox.SelectedIndex]._gameArgs;
                    // Enter the executable to run, including the complete path
                    start.FileName = gameList[gameListBox.SelectedIndex]._gameLocation;
                }

                // Do you want to show a console window?
                start.WindowStyle = ProcessWindowStyle.Hidden;
                start.CreateNoWindow = true;
                int exitCode;

                Console.WriteLine("Booting " + gameList[gameListBox.SelectedIndex]._gameName + "....");
                client.SetPresence(new RichPresence()
                {
                    Details = "Version " + Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    State = "Playing " + gameList[gameListBox.SelectedIndex]._gameName,
                    Assets = new Assets()
                    {
                        LargeImageKey = "image_large",
                        LargeImageText = "LAN Game Manager by nzgamer41",
                        SmallImageKey = "image_small"
                    }
                });
                // Run the external process & wait for it to finish
                using (Process proc = Process.Start(start))
                {
                    proc.WaitForExit();

                    // Retrieve the app's exit code
                    exitCode = proc.ExitCode;
                }

                waveOut.Play();
                Console.WriteLine("Booting " + gameList[gameListBox.SelectedIndex]._gameName + "....");
                client.SetPresence(new RichPresence()
                {
                    Details = "Version " + Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    State = "",
                    Assets = new Assets()
                    {
                        LargeImageKey = "image_large",
                        LargeImageText = "LAN Game Manager by nzgamer41",
                        SmallImageKey = "image_small"
                    }
                });
            }
            else
            {
                MessageBox.Show("Game doesn't appear to be installed, you should probably download it first lmao");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (gameListBox.Items.Count == 0)
            {
                MessageBox.Show("You have no games installed.");
                return;
            }
            var w = new ConfigWindow(gameList[gameListBox.SelectedIndex]);
            if (w.ShowDialog() == true)
            {
                Game updatedGame = w._selectedGame;
                gameList[gameListBox.SelectedIndex] = updatedGame;
                Helpers.WriteToXmlFile("games.xml", basegameList, false);
                if (updatedGame._isInstalled)
                {
                    downloadButton.IsEnabled = false;
                    launchButton.IsEnabled = true;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Helpers.WriteToXmlFile("games.xml", basegameList, false);
            waveOut.Dispose();
            this.Close();
        }
        
        
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (!_appSettings.offlineMode)
            {
                InstallMenu installMenu = new InstallMenu(gameList);
                installMenu.ShowDialog();
                foreach (var t in installMenu._gameList)
                {
                    if (basegameList.Contains(t))
                    {
                        for (int i = 0; i < basegameList.Count; i++)
                        {
                            if (basegameList[i]._gameName == t._gameName)
                            {
                                basegameList[i] = t;
                            }
                        }
                    }
                    else
                    {
                        if (t._isInstalled)
                        {
                            basegameList.Add(t);
                        }
                    }
                }

                foreach (var t in basegameList)
                {
                    if (t._isInstalled)
                    {
                        if (!gameListBox.Items.Contains(t))
                        {
                            gameListBox.Items.Add(t);
                            gameList.Add(t);
                        }
                    }
                }

                //Helpers.WriteToXmlFile("games.xml", basegameList, false);
                if (gameListBox.Items.Count != 0)
                {
                    gameListBox.SelectedIndex = 0;
                }
            }
            else
            {
                MessageBox.Show("LANGameManager has been configured into offline mode, so downloading is disabled.",
                    "LANGameManager Error");
            }
        }

        public void ExecuteAsAdmin(string fileName, string commandLine)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = commandLine;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        private void ButtonMusicToggle_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Stop();
                downloadButton_Copy.Content = "Play Music";
            }
            else if (waveOut.PlaybackState == PlaybackState.Stopped)
            {
                waveOut.Play();
                downloadButton_Copy.Content = "Stop Music";
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var w = new About();
            w.ShowDialog();
        }

        private void DebugButton1_OnClick(object sender, RoutedEventArgs e)
        {
#if DEBUG
            for (int i = 0; i < 10; i++)
            {
                Game testGame = new Game();
                testGame._gameName = "Debug Game " + i;
                testGame._isInstalled = true;
                gameListBox.Items.Add(testGame);
            }
#endif
        }
    }
}
