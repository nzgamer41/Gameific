using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace GameificClient
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public Game _selectedGame;
        public ConfigWindow(Game selectedGame)
        {
            InitializeComponent();
            _selectedGame = selectedGame;
            textBlockGamePath.Text = _selectedGame._gameLocation;
        }

        private void TextBlockGamePath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                textBlockGamePath.Text = openFileDialog.FileName;
                _selectedGame._gameLocation = openFileDialog.FileName;
                _selectedGame._isInstalled = true;
            }
        }
    }
}
