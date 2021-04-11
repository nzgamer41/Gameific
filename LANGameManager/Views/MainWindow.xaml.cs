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
using System.Windows.Forms;
using System.Windows.Media.Imaging;



namespace GameificClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User _loggedIn;
        public MainWindow(User loggedInUser)
        {
            InitializeComponent();

            Game game1 = new Game();
            game1._gameName = "Game 1";
            game1._gameLocation = "C:\\Windows\\System32";

            gameListBox.Items.Add(game1);
            gameListBox.Items.Add("Game 2");
            gameListBox.Items.Add("Game 3");
            _loggedIn = loggedInUser;
            this.Title += " logged in as " + _loggedIn._username;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow cw = new ConfigWindow((Game)gameListBox.SelectedItem);
            cw.ShowDialog();
        }
    }
}
