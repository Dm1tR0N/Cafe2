using System;
using System.Diagnostics;
using System.Windows;
using Cafe2.Functions;

namespace Cafe2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dbContext = new PostgresContext())
            {
                var cafeAuth = new AuthenticationManager(dbContext);

                if (!string.IsNullOrEmpty(strLogin.Text) && !string.IsNullOrEmpty(strPassword.Text))
                {
                    if (cafeAuth.AuthenticateUser(strLogin.Text, strPassword.Text))
                    {
                        // Пользователь успешно аутентифицирован
                        Debug.WriteLine("Вход выполнен успешно!");
                        LoginWin.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        // Неверные учетные данные
                        Debug.WriteLine("Неверный логин или пароль.");
                        strLogin.Text = "";
                        strPassword.Text = "";
                    }
                }
            }
        }
    }
}
