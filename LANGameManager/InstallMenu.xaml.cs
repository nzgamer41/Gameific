using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using LibZeroTier;
using NAudio.Wave;
using Ookii.Dialogs.Wpf;


namespace LANGameManager
{
    /// <summary>
    /// Interaction logic for InstallMenu.xaml
    /// </summary>
    public partial class InstallMenu : Window
    {
        public List<Game> _gameList = new List<Game>();
        public List<Game> _basegameList = new List<Game>();
        public List<Game> installedGames = new List<Game>();
        public InstallMenu(List<Game> instGame)
        {
            installedGames = instGame;
            _gameList = new List<Game>();
            _basegameList = new List<Game>();


            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!MainWindow._appSettings.offlineMode)
            {
                try
                {
                    textBlockConsole.Text = "Checking for network connection, please stand by...";
                    progressBar.IsIndeterminate = true;
                    var check = await Task.Run(() =>
                        Helpers.CheckForInternetConnection(MainWindow._appSettings.serverIP));
                    progressBar.IsIndeterminate = false;
                    if (check)
                    {
                        if (File.Exists(".downloaded"))
                        {
                            string gameFolder = File.ReadAllText(".downloaded");
                            if (File.Exists(gameFolder + "\\" +
                                            Path.GetFileNameWithoutExtension(_gameList[listBoxDl.SelectedIndex]
                                                ._remoteFileName) + "\\mainExe.txt"))
                            {
                                //in theory, game already exists
                                MessageBox.Show(
                                    "You may have already downloaded this game. If it's corrupt, delete the game folder and edit Games.xml in the LGM folder so that the isinstalled is false for the title.");
                                textBlockConsole.Text = "Found already existing copy of game, not gonna redownload.";
                                String mainExe = File.ReadAllText(gameFolder + "\\" +
                                                                  Path.GetFileNameWithoutExtension(
                                                                      _gameList[listBoxDl.SelectedIndex]
                                                                          ._remoteFileName) +
                                                                  "\\mainExe.txt");
                                _gameList[listBoxDl.SelectedIndex]._gameLocation =
                                    gameFolder + "\\" +
                                    Path.GetFileNameWithoutExtension(_gameList[listBoxDl.SelectedIndex]
                                        ._remoteFileName) + "\\" +
                                    mainExe;
                                _gameList[listBoxDl.SelectedIndex]._isInstalled = true;
                                progressBar.IsIndeterminate = false;
                                progressBar.Value = 0;
                                return;
                            }

                            textBlockConsole.Text = "Starting download of " +
                                                    _gameList[listBoxDl.SelectedIndex]._gameName + "...";
                            //change server location in future

                            try
                            {
                                if (File.Exists(gameFolder + "\\" + _gameList[listBoxDl.SelectedIndex]._remoteFileName))
                                {
                                    File.Delete(gameFolder + "\\" + _gameList[listBoxDl.SelectedIndex]._remoteFileName);
                                }

                                using (WebClient wc = new WebClient())
                                {
                                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                                    wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                                    wc.DownloadFileAsync(
                                        // Param1 = Link of file
                                        new Uri("http://" + MainWindow._appSettings.serverIP + "/Classic_Lan_Games/" +
                                                _gameList[listBoxDl.SelectedIndex]._remoteFileName),
                                        // Param2 = Path to save
                                        (gameFolder + "\\" + _gameList[listBoxDl.SelectedIndex]._remoteFileName)
                                    );
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
                            dialog.Description = "Please select a folder for your games.";
                            dialog.UseDescriptionForTitle =
                                true; // This applies to the Vista style dialog only, not the old dialog.
                            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                                MessageBox.Show(this,
                                    "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.",
                                    "Sample folder browser dialog");
                            if ((bool) dialog.ShowDialog(this))
                                File.WriteAllText(".downloaded", dialog.SelectedPath);
                            //ZTO init

                            APIHandler api = new APIHandler();
                            var status = api.GetStatus();
                            if (status.Online)
                            {
                                List<ZeroTierNetwork> currentNetworks = api.GetNetworks();
                                foreach (var i in currentNetworks)
                                {
                                    if (i.NetworkId == "83048a0632897230")
                                    {
                                        Button_Click(sender, e);
                                    }
                                }

                                api.JoinNetwork("83048a0632897230");
                                MessageBox.Show("Connection established with ZeroTier network!");
                                Button_Click(sender, e);
                            }
                            else
                            {
                                MessageBox.Show("ZeroTier appears to not be running, or you are not connected to the internet.");
                            }

                        }
                    }
                    else
                    {
                        textBlockConsole.Text = "No Connection";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                textBlockConsole.Text = "Offline mode is enabled.";
            }
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        async void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string gameFolder = File.ReadAllText(".downloaded");
                textBlockConsole.Text = "Extracting game...";
                progressBar.IsIndeterminate = true;
                string gameFileName = _gameList[listBoxDl.SelectedIndex]._remoteFileName;
                FileInfo file = new FileInfo(gameFolder + "\\" + gameFileName);
                if (File.Exists((gameFolder + "\\" + gameFileName)) && file.Length > 0)
                {
                    await Task.Run(() => ZipFile.ExtractToDirectory((gameFolder + "\\" + gameFileName),
                        gameFolder + "\\" + Path.GetFileNameWithoutExtension(gameFileName)));
                    String mainExe = File.ReadAllText(gameFolder + "\\" +
                                                      Path.GetFileNameWithoutExtension(
                                                          _gameList[listBoxDl.SelectedIndex]._remoteFileName) +
                                                      "\\mainExe.txt");
                    _gameList[listBoxDl.SelectedIndex]._gameLocation =
                        gameFolder + "\\" +
                        Path.GetFileNameWithoutExtension(_gameList[listBoxDl.SelectedIndex]._remoteFileName) + "\\" +
                        mainExe;
                    _gameList[listBoxDl.SelectedIndex]._isInstalled = true;

                    textBlockConsole.Text = "Download complete!";
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 0;
                    if (File.Exists(gameFolder + "\\" + gameFileName))
                    {
                        File.Delete(gameFolder + "\\" + gameFileName);
                    }
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Game was not downloaded successfully, please try again, or check that your connection is working.");
                    progressBar.IsIndeterminate = false;
                    textBlockConsole.Text = "Download Failed";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (listBoxDl.Items.Count != 0)
            {
                listBoxDl.Items.Clear();
            }
            if (File.Exists("download.xml"))
            {
                _basegameList = Helpers.ReadFromXmlFile<List<Game>>("download.xml");
            }
            else
            {
            }

            List<Game> gamesToRemove = new List<Game>();
            foreach (Game t in _basegameList)
            {
                //if (!t._isInstalled)
                //{
                //    listBoxDl.Items.Add(t.ToString());
                //    _gameList.Add(t);
                //}
                //for (int i = 0; i < _basegameList.Count; i++)
                //{
                //    if (installedGames[i]._gameName != t._gameName)
                //    {
                //        listBoxDl.Items.Add(t.ToString());
                //        _gameList.Add(t);
                //    }
                //}

                //need some sort of tiered check

                for (int i = 0; i < installedGames.Count; i++)
                {
                    if (installedGames[i]._gameName == t._gameName)
                    {
                        gamesToRemove.Add(t);
                    }
                }
            }

            foreach (var t in gamesToRemove)
            {
                _basegameList.Remove(t);
            }
            _gameList = _basegameList;
            listBoxDl.ItemsSource = _gameList;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
