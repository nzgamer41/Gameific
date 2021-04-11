using System;
using System.Collections.Generic;
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

namespace GameificClient
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
#if DEBUG
            bool online = Helpers.CheckForInternetConnection("127.0.0.1");
#endif
            if (!online)
            {
                MainWindow mw = new MainWindow(new User());
                mw.Show();
                this.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                User temp = new User();
                temp.logon(textBoxUser.Text, passwordBox.Password);
                MainWindow mw = new MainWindow(temp);
                mw.Show();
                this.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow(new User());
            mw.Show();
            this.Close();
        }
    }
}
