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

            gameListBox.Items.Add("Game 1");
            gameListBox.Items.Add("Game 2");
            gameListBox.Items.Add("Game 3");

        }
    }
}
