using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cafe2.Functions;
using Cafe2.Models;
using Microsoft.EntityFrameworkCore;

namespace Cafe2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static object userData;
        private readonly PostgresContext _dbContext;

        public class OrderViewModel
        {
            public int OrderID { get; set; }
            public int TableNumber { get; set; }
            public DateTime DateOrder { get; set; }
            public int DishID { get; set; }
            public string DishName { get; set; }
            public int DishCount { get; set; }
            public int IdReadyStatus { get; set; }
            public int IdPaymentStatus { get; set; }

            public int IdPaymentMethod { get; set; }
        }

        public class DetailOrderViewModel
        {
            public int DishID { get; set; }
            public string DishName { get; set; }
            public int DishCount { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();

            _dbContext = new PostgresContext();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dbContext = new PostgresContext())
            {

                if (!string.IsNullOrEmpty(strLogin.Text) && !string.IsNullOrEmpty(strPassword.Text))
                {
                   var user = _dbContext.Users.FirstOrDefault(u => u.Login == strLogin.Text && u.Password == strPassword.Text);
                   userData = user;

                    if (user.Idgroup == 1)
                    {
                        // Пользователь успешно аутентифицирован
                        InfoLabel.Content = $"Добро пожаловать!";
                        LoginWin.Visibility = Visibility.Hidden;
                        AdminPanel.Visibility = Visibility.Visible;

                        var query = dbContext.Orders
                        .Include(o => o.DishInOrders)
                            .ThenInclude(dio => dio.IddishNavigation)
                        .SelectMany(o => o.DishInOrders.Select(dio => new OrderViewModel
                        {
                            OrderID = o.Idorder,
                            TableNumber = o.IdtableNumber,
                            DateOrder = o.DateOrder.Date, // DateTime или другой тип по вашему выбору
                            DishID = dio.Iddish,
                            DishName = dio.IddishNavigation.NameDish,
                            DishCount = dio.Count,
                            IdReadyStatus = o.IdreadyStatus,
                            IdPaymentStatus = o.IdpaymentStatus,
                            IdPaymentMethod = o.IdpaymentMethod

                        })).ToList();

                        // Теперь присваиваем данные к ItemsSource вашего DataGrid
                        ListOrders.ItemsSource = query;
                    }
                    else
                    {
                        // Неверные учетные данные
                        InfoLabel.Content = "Неверный логин или пароль.";
                        strLogin.Text = "";
                        strPassword.Text = "";
                    }
                }
            }
        }

        private void ListOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListOrders.SelectedItem != null)
            {
                // Получаем выбранный элемент
                OrderViewModel selectedOrder = (OrderViewModel)ListOrders.SelectedItem;

                // Отображаем информацию в DeatailOrder
                ShowOrderDetails(selectedOrder.OrderID);
            }
        }

        private void ShowOrderDetails(int orderId)
        {
            using (var dbContext = new PostgresContext())
            {
                // Здесь выполняете запрос для получения деталей заказа по orderId
                var orderDetails = dbContext.DishInOrders
                    .Where(dio => dio.Idorder == orderId)
                    .Select(dio => new DetailOrderViewModel
                    {
                        DishID = dio.Iddish,
                        DishName = dio.IddishNavigation.NameDish,
                        DishCount = dio.Count
                    })
                    .ToList();

                // Теперь присваиваем данные к ItemsSource DeatailOrder
                DeatailOrder.ItemsSource = orderDetails;

                // Отображаем DeatailOrder и скрываем ListOrders
                ListOrders.Visibility = Visibility.Hidden;
                DeatailOrder.Visibility = Visibility.Visible;
            }
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            ListOrders.Visibility = Visibility.Visible;
            DeatailOrder.Visibility = Visibility.Hidden;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
