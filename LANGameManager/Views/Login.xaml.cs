using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                labelUser.Visibility = Visibility.Collapsed;
                labelPw.Visibility = Visibility.Collapsed;
                textBoxUser.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Collapsed;
                buttonCancel.Visibility = Visibility.Collapsed;
                buttonLogin.Visibility = Visibility.Collapsed;
                buttonRegister.Visibility = Visibility.Collapsed;
                pbLogin.Visibility = Visibility.Visible;
                pbLogin.IsIndeterminate = true;

                User temp = new User();
                bool success = false;
                string un = textBoxUser.Text;
                string pw = passwordBox.Password;
                await Task.Run(() =>{
                    success = temp.logon(un, pw);
                });
                if (success)
                {
                    MainWindow mw = new MainWindow(temp);
                    mw.Show();
                    this.Close();
                }
                else
                {
                    labelUser.Visibility = Visibility.Visible;
                    labelPw.Visibility = Visibility.Visible;
                    textBoxUser.Visibility = Visibility.Visible;
                    passwordBox.Visibility = Visibility.Visible;
                    buttonCancel.Visibility = Visibility.Visible;
                    buttonLogin.Visibility = Visibility.Visible;
                    buttonRegister.Visibility = Visibility.Visible;
                    pbLogin.Visibility = Visibility.Collapsed;
                    pbLogin.IsIndeterminate = false;
                    MessageBox.Show("Failed to log in!");
                }

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

        private async void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                User newUser = new User(textBoxUser.Text, passwordBox.Password);
                


                labelUser.Visibility = Visibility.Collapsed;
                labelPw.Visibility = Visibility.Collapsed;
                textBoxUser.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Collapsed;
                buttonCancel.Visibility = Visibility.Collapsed;
                buttonLogin.Visibility = Visibility.Collapsed;
                buttonRegister.Visibility = Visibility.Collapsed;
                pbLogin.Visibility = Visibility.Visible;
                pbLogin.IsIndeterminate = true;

                User temp = new User();
                bool success = false;
                string un = textBoxUser.Text;
                string pw = passwordBox.Password;
                await Task.Run(() => {
                    temp = Networking.register(newUser, "127.0.0.1");
                    success = temp.logon(un, pw);
                });
                if (success)
                {
                    MainWindow mw = new MainWindow(temp);
                    mw.Show();
                    this.Close();
                }
                else
                {
                    labelUser.Visibility = Visibility.Visible;
                    labelPw.Visibility = Visibility.Visible;
                    textBoxUser.Visibility = Visibility.Visible;
                    passwordBox.Visibility = Visibility.Visible;
                    buttonCancel.Visibility = Visibility.Visible;
                    buttonLogin.Visibility = Visibility.Visible;
                    buttonRegister.Visibility = Visibility.Visible;
                    pbLogin.Visibility = Visibility.Collapsed;
                    pbLogin.IsIndeterminate = false;
                    MessageBox.Show("Failed to register!");
                }
            }
            catch
            {
                labelUser.Visibility = Visibility.Visible;
                labelPw.Visibility = Visibility.Visible;
                textBoxUser.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Visible;
                buttonCancel.Visibility = Visibility.Visible;
                buttonLogin.Visibility = Visibility.Visible;
                buttonRegister.Visibility = Visibility.Visible;
                pbLogin.Visibility = Visibility.Collapsed;
                pbLogin.IsIndeterminate = false;
                MessageBox.Show("Failed to register user!");
            }
        }
    }
}
