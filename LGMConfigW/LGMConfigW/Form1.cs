using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LGMConfigW.Classes;
using MonoTorrent;
using Ookii.Dialogs.WinForms;
using ProgressBarStyle = System.Windows.Forms.ProgressBarStyle;

namespace LGMConfigW
{
    public partial class Form1 : Form
    {
        BindingList<Game> binGen = new BindingList<Game>();
        public Form1()
        {
            InitializeComponent();
            listBox1.DataSource = binGen;
        }

     
        private async void buttonAddGame_Click(object sender, EventArgs e)
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            await zipGame(textBoxDir.Text, textBoxGameName.Text);
            progressBar1.Style = ProgressBarStyle.Blocks;
            if (textBoxGameName.Text == "")
            {
                MessageBox.Show("Invalid Game Name");
                return;
            }

            Game ng = new Game();
            ng._gameName = textBoxGameName.Text;
            ng._gameArgs = textBoxLA.Text;
            ng._remoteFileName = textBoxGameName.Text + ".zip";
            ng.relPathToExe = textBoxExec.Text;
            binGen.Add(ng);
            CreateTorrent(textBoxGameName.Text + ".zip", textBoxGameName.Text + ".torrent");

            textBoxGameName.Text = "";
            textBoxLA.Text = "";
            textBoxDir.Text = "";
            textBoxExec.Text = "";
            listBox1.SelectedIndex = -1;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                Game temp = (Game)listBox1.SelectedItem;
                binGen.Remove(temp);
            }
        }

        private static async Task zipGame(string dir, string output)
        {
            await Task.Run(() =>
            {
                ZipFile.CreateFromDirectory(dir, output +".zip");
            });
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Game.json|Game.json|All Files|*.*";
            openFileDialog1.FileName = "";
            openFileDialog1.Title = "Open a previously generated Game.bin file";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                binGen = Helpers.ReadFromJsonFile<BindingList<Game>>(openFileDialog1.FileName);
            }

            listBox1.DataSource = binGen;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Game.json|Game.json|All Files|*.*";
            saveFileDialog1.FileName = "";
            saveFileDialog1.Title = "Choose where to save the generated Game.json file:";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Helpers.WriteToJsonFile(saveFileDialog1.FileName, binGen, false);
                binGen.Clear();
                listBox1.DataSource = binGen;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Game selGame = (Game) listBox1.SelectedItem;
            if (selGame != null)
            {
                textBoxGameName.Text = selGame._gameName;
                textBoxLA.Text = selGame._gameArgs;
                textBoxExec.Text = selGame.relPathToExe;
            }
        }




        // 'path' is the location of the file/folder which is going to be converted
         // to a torrent. 'savePath' is where the .torrent file will be saved.
         async void CreateTorrent(string path, string savePath)
         {
             string serverIP = "";
             if (File.Exists("trackerip.txt"))
             {
                 serverIP = File.ReadAllText("trackerip.txt");
             }
             else
             {
                 MessageBox.Show(
                     "You have not set an IP for the tracker! Please set that in File -> Configure Torrent Generator!",
                     "Settings invalid!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 return;
             }


             TorrentCreator c = new TorrentCreator();

             // Fill in the path, trackers as in Example 1
             // Add one tier which contains two trackers
             RawTrackerTier tier = new RawTrackerTier();
             tier.Add("http://" + serverIP + ":6969/announce/");
             tier.Add("udp://" + serverIP + ":10001");

             c.Announces.Add(tier);
             c.Comment = "Generated by LGMConfigW";
             c.CreatedBy = "LGMConfigW using MonoTorrent" + VersionInfo.Version;
             c.Publisher = "http://nzgamer41.win";

             // Set the torrent as private so it will not use DHT or peer exchange
             // Generally you will not want to set this.
             c.Private = true;

             ITorrentFileSource fileSource = new TorrentFileSource(path);

             // Create the torrent asynchronously
             await c.CreateAsync(fileSource, savePath);
         }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void configureTorrentGeneratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TorGenDlgBx tg = new TorGenDlgBx();
            tg.ShowDialog();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            openFileDialog1.Filter = "Game Executable|*.exe|All Files|*.*";
            openFileDialog1.FileName = "";
            openFileDialog1.Title = "Select executable file:";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string exename = openFileDialog1.FileName;
                VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
                fbd.SelectedPath = Path.GetDirectoryName(openFileDialog1.FileName);
                fbd.Description = "Select root folder";
                fbd.UseDescriptionForTitle = true;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string rootDir = fbd.SelectedPath;

                    textBoxDir.Text = rootDir;
                    textBoxExec.Text = exename.Replace(rootDir, "");
                }
            }
        }
    }
}
