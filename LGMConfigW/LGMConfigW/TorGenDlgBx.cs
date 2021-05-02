using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LGMConfigW
{
    public partial class TorGenDlgBx : Form
    {
        public TorGenDlgBx()
        {
            InitializeComponent();
        }

        private void TorGenDlgBx_Load(object sender, EventArgs e)
        {
            if (File.Exists("trackerip.txt"))
            {
                string trkIP = File.ReadAllText("trackerip.txt");
                textBoxTrackerIP.Text = trkIP;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (File.Exists("trackerip.txt"))
            {
                File.Delete("trackerip.txt");
            }
            File.WriteAllText("trackerip.txt", textBoxTrackerIP.Text);
            this.Close();
        }
    }
}
