using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cafe2.Functions;
using Cafe2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.ObjectModel;
using System.Globalization;

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
        
        public static int ordersId { get; set; }

        public static byte[] photoEmployee { get; set; }
        public static byte[] photoContract { get; set; }

        public static User selectEditUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _dbContext = new PostgresContext();
            GetGroupsList();
        }
        public void GetGroupsList()
        {
            using (var dbContext = new PostgresContext())
            {
                var groupNames = dbContext.Groups.Select(g => g.NameGroup).ToList();
                groupList_User.ItemsSource = groupNames;
            }
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
                        //InfoLabel.Content = $"Добро пожаловать!";
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
                    else if (user.Idgroup == 2)
                    {
                        //InfoLabel.Content = "Добро пожаловать Оффициант";
                    }
                    else if (user.Idgroup == 3)
                    {
                        //InfoLabel.Content = "Добро пожаловать Повар";
                    }
                    else if (user == null)
                    {
                        //InfoLabel.Content = "пользователь не найден!";
                        strLogin.Text = "";
                        strPassword.Text = "";
                    }
                    else
                    {
                        // Неверные учетные данные
                        //InfoLabel.Content = "Неверный логин или пароль.";
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
            ordersId = orderId;
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

                var menu = dbContext.Menus.ToList();
                decimal price = 0;
                foreach (var item in menu)
                {
                    foreach (var item1 in orderDetails)
                    {
                        if (item.Iddish == item1.DishID)
                        {
                            if (item1.DishCount > 0)
                            {
                                price = price + (item.Price * item1.DishCount);
                            }
                        }
                    }
                }

                var needidOrder = dbContext.DishInOrders.FirstOrDefault(x => x.Idorder == orderId);
                var idorder = dbContext.Orders.FirstOrDefault(o => o.Idorder == needidOrder.Idorder);

                var statusReady = dbContext.PymentStatuses.FirstOrDefault(o => o.IdpymentStatus == idorder.IdpaymentStatus);

                innfoOrder.Content = $"Итогыйвый чек заказа: {price} рублей, \nСтатус оплаты: {statusReady.Name}";

                if (statusReady.IdpymentStatus == 2)
                {
                    AddOrderPanel.Visibility = Visibility.Visible;
                    btnSave.Visibility = Visibility.Visible;
                }

                var menuList = dbContext.Menus.ToList();
                listMenuAdd.ItemsSource = menuList.Select(x => x.NameDish);

                // Отображаем DeatailOrder и скрываем ListOrders
                DeatailOrder_Loaded_Height();
                ListOrders.Visibility = Visibility.Hidden;
                DeatailOrder.Visibility = Visibility.Visible;
            }
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            ListOrders.Visibility = Visibility.Visible;
            DeatailOrder.Visibility = Visibility.Hidden;

            AddOrderPanel.Visibility = Visibility.Hidden;
            btnSave.Visibility = Visibility.Hidden;

            innfoOrder.Content = $"Итогыйвый чек заказа: 0 рублей";
        }

        private void Reg_btn(object sender, RoutedEventArgs e)
        {
            AdminPanel.Visibility = Visibility.Hidden;
            RegWindow.Visibility = Visibility.Visible;
        }

        private void Back_adm_click(object sender, RoutedEventArgs e)
        {
            AdminPanel.Visibility = Visibility.Visible;
            RegWindow.Visibility = Visibility.Hidden;
        }
        public class PhotoSelector
        {
            public string SelectPhoto()
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Изображения (*.jpg; *.png; *.gif)|*.jpg;*.png;*.gif|Все файлы (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    return openFileDialog.FileName;
                }

                return null;
            }
        }

        private void view_photo_employee(object sender, RoutedEventArgs e)
        {
            PhotoSelector photoSelector = new PhotoSelector();
            string selectedFilePath = photoSelector.SelectPhoto();

            if (selectedFilePath != null)
            {
                using (FileStream stream = new FileStream(selectedFilePath, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader reader = new BinaryReader(stream);
                    photoEmployee = reader.ReadBytes((int)stream.Length);
                }
                MessageBox.Show($"{Convert.ToString(photoEmployee)}", $"");
            }
        }

        private void view_photo_contract(object sender, RoutedEventArgs e)
        {
            PhotoSelector photoSelector = new PhotoSelector();
            string selectedFilePath = photoSelector.SelectPhoto();

            if (selectedFilePath != null)
            {
                using (FileStream stream = new FileStream(selectedFilePath, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader reader = new BinaryReader(stream);
                    photoContract = reader.ReadBytes((int)stream.Length);
                }
                //MessageBox.Show($"{Convert.ToString(photoContract)}", $"");
            }
        }

        public int getIdGroup(string groupName)
        {
            using (var dbContext = new PostgresContext())
            {
                var idGroup = dbContext.Groups.Where(x => x.NameGroup == groupName).FirstOrDefault();

                return idGroup.Idgroup;
            }
        }

        private void create_user_btn(object sender, RoutedEventArgs e)
        {
            PostgresContext context = new PostgresContext();
            UserDocument document = new UserDocument();
            User user = new User();
            HistoryUser historyUser = new HistoryUser();

            UserDocument newDocument = new UserDocument()
            {
                ScanContract = photoContract
            };
            context.UserDocuments.Add(newDocument);
            context.SaveChanges();

            User newUser = new User()
            {
                FirstName = firstName_User.Text,
                SecondName = lasttName_User.Text,
                MiddleName = middleName_User.Text,
                Login = login_User.Text,
                Password = password_User.Password,
                Photo = photoEmployee,
                Iddocuments = newDocument.IduserDocument,
                Idgroup = getIdGroup(groupList_User.Text)
            };
            context.Users.Add(newUser);
            context.SaveChanges();

            HistoryUser newString = new HistoryUser()
            {
                Iduser = newUser.Iduser,
                Status = 2,
                Date = DateTime.UtcNow
            };
            context.HistoryUsers.Add(newString);
            context.SaveChanges();

            if (newDocument != null &&  newUser != null && newString != null)
            {
                AdminPanel.Visibility = Visibility.Visible;
                RegWindow.Visibility = Visibility.Hidden;
            }
        }

        public void GetUsersList()
        {
            using (var dbcontext = new PostgresContext())
            {
                var users = dbcontext.Users.ToList();

                // Здесь мы используем интерполяцию строк с помощью символа $ перед началом строки и фигурных скобок {} для вставки свойств в строку.
                ListEmployees_EditUser.ItemsSource = users.Select(x => x.Login);
            }
        }

        private void editUserDate(object sender, RoutedEventArgs e)
        {
            GetUsersList();
            
            AdminPanel.Visibility = Visibility.Hidden;
            editUserWindow.Visibility = Visibility.Visible;
            
        }

        public int GetIdUser()
        {
            using( var dbcontext = new PostgresContext())
            {
                var user = dbcontext.Users.FirstOrDefault(x => x.Login == ListEmployees_EditUser.Text);

                return user.Iduser;
            }
        }

        public static Image ByteArrayToImage(byte[] imageBytes)
        {
            // Создаем объект BitmapImage
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(imageBytes);
            bitmapImage.EndInit();

            // Создаем объект Image
            Image imageControl = new Image();
            imageControl.Source = bitmapImage;

            return imageControl;
        }

        public void loadingDataInEditUserWindow()
        {
            using( var dbcontext = new PostgresContext())
            {
                var user = dbcontext.Users.FirstOrDefault(x => x.Iduser == GetIdUser());
                selectEditUser = user;

                editUserFirstName.Text = user.FirstName;
                editUserSecondName.Text = user.SecondName;
                editUserMiddleName.Text = user.MiddleName;
                listStatusUser.ItemsSource = dbcontext.EmployeeStatuses.Select(x => x.NameStatus).ToList();
                // Проверяем, что user.Photo не null
                if (user.Photo != null)
                {
                    try
                    {
                        // Преобразуем строку Base64 в массив байтов
                        byte[] photoBytes = user.Photo;

                        // Создаем объект BitmapImage
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = new MemoryStream(photoBytes);
                        bitmapImage.EndInit();

                        // Устанавливаем источник изображения для imageEmployee
                        imageEmployee.Source = bitmapImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обработке изображения: {ex.Message}");
                    }
                }
                else
                {
                    var exePath = AppDomain.CurrentDomain.BaseDirectory;//path to exe file
                    var path = Path.Combine(exePath, "Images\\nonphoto.jpg");

                    // Создаем объект BitmapImage
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(path, UriKind.Absolute);
                    bitmapImage.EndInit();

                    // Устанавливаем источник изображения для imageEmployee
                    imageEmployee.Source = bitmapImage;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SelectEmployeeWindow.Visibility = Visibility.Hidden;
            EditUserWindow.Visibility = Visibility.Visible;
            loadingDataInEditUserWindow();
        }


        private void cancelEditWindow(object sender, RoutedEventArgs e)
        {
            editUserWindow.Visibility = Visibility.Hidden;
            EditUserWindow.Visibility = Visibility.Hidden;
            AdminPanel.Visibility = Visibility.Visible;
            SelectEmployeeWindow.Visibility = Visibility.Visible;
        }


        private void saveEditUser(object sender, RoutedEventArgs e)
        {
            User user = new User();
            HistoryUser historyUser = new HistoryUser();

            using(var dbContext = new PostgresContext())
            {
                var userEd = dbContext.Users.FirstOrDefault(x => x.Iduser == selectEditUser.Iduser);
                var userInHistory = dbContext.HistoryUsers;

                userEd.FirstName = editUserFirstName.Text;
                userEd.SecondName = editUserSecondName.Text;
                userEd.MiddleName = editUserMiddleName.Text;
                dbContext.SaveChanges();

                var status = dbContext.EmployeeStatuses.FirstOrDefault(x => x.NameStatus == listStatusUser.Text);
                
                var stingUser = dbContext.HistoryUsers
                        .OrderByDescending(x => x.Date)
                        .FirstOrDefault(x => x.Iduser == userEd.Iduser);

                var fin = dbContext.EmployeeStatuses.FirstOrDefault(x => x.IdemployeeStatus == stingUser.Status);

                if (listStatusUser.Text != fin.NameStatus && listStatusUser.Text != null)
                {
                    var statusId = dbContext.EmployeeStatuses.FirstOrDefault(x => x.NameStatus == listStatusUser.Text);
                    HistoryUser newString = new HistoryUser() 
                    {
                        Iduser = userEd.Iduser,
                        Status = statusId.IdemployeeStatus,
                        Date = DateTime.UtcNow
                    };
                    dbContext.HistoryUsers.Add(newString);
                    dbContext.SaveChanges();
                }

                AdminPanel.Visibility = Visibility.Visible;
                editUserWindow.Visibility = Visibility.Hidden;
                SelectEmployeeWindow.Visibility = Visibility.Hidden;
            }

        }

        public void DeatailOrder_Loaded_Height()
        {
            // Рассчитываем высоту на основе числа строк
            int rowCount = DeatailOrder.Items.Count;

            // Задаем высоту
            double rowHeight = 30; // Замените на фактическую высоту строки
            double desiredHeight = Math.Min(rowCount * rowHeight, SystemParameters.PrimaryScreenHeight); // Ограничиваем высоту экрана, чтобы избежать неудобств

            DeatailOrder.MaxHeight = desiredHeight;
        }

        private void addCountOrder(object sender, RoutedEventArgs e)
        {
            int num = Convert.ToInt32(CountOrderItem.Text);
            if (num != 5)
            {
                num += 1;
                CountOrderItem.Text = Convert.ToString(num);
            }
        }

        private void munusCountOrder(object sender, RoutedEventArgs e)
        {
            int num = Convert.ToInt32(CountOrderItem.Text);
            if (num != 1)
            {
                num -= 1;
                CountOrderItem.Text = Convert.ToString(num);
            }
        }

        private void AddDishInOrder(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dbContext = new PostgresContext())
                {
                    var dishes = dbContext.Menus
                        .FirstOrDefault(x => x.NameDish == listMenuAdd.Text);
                    DishInOrder dishInOrder = new DishInOrder();

                    DishInOrder newString = new DishInOrder()
                    {
                        Idorder = ordersId,
                        Iddish = dishes.Iddish,
                        Count = Convert.ToInt32(CountOrderItem.Text)
                    };
                    dbContext.DishInOrders.Add(newString);
                    dbContext.SaveChanges();
                }
                ShowOrderDetails(ordersId);
                MessageBox.Show($"Товар {listMenuAdd.Text} добавен к заказу!", "Добавлен");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка");
            }
        }

        private void settingsBtn_addWorksShift_Click(object sender, RoutedEventArgs e)
        {
            using(var dbContext = new PostgresContext())
            {
                TypeWorksShift typeWorksShift = new TypeWorksShift();

                // Получаем выбранную дату из DatePicker
                DateTime dateWorkStart = settingsBtn_dateWorking.SelectedDate ?? DateTime.UtcNow;
                DateTime dateWorkEnd = settingsBtn_dateWorking.SelectedDate ?? DateTime.UtcNow;

                // Получаем выбранное время из ComboBox
                string startHour = settingsBtn_startDate.Text;
                string endHour = settingsBtn_endDate.Text;

                // Разбираем строку времени и добавляем к дате
                if (DateTime.TryParseExact(startHour, "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
                {
                    dateWorkStart = dateWorkStart.AddHours(parsedTime.Hour).AddMinutes(parsedTime.Minute).ToUniversalTime();
                }

                if (DateTime.TryParseExact(endHour, "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime1))
                {
                    dateWorkEnd = dateWorkEnd.AddHours(parsedTime1.Hour).AddMinutes(parsedTime1.Minute).ToUniversalTime();
                }

                TypeWorksShift newType = new TypeWorksShift() 
                {
                    Start = dateWorkStart,
                    End = dateWorkEnd,
                    WorkingRate = Convert.ToDecimal(settingsBtn_WorkingRate.Text)
                };
                dbContext.TypeWorksShifts.Add(newType);
                dbContext.SaveChanges();
                MessageBox.Show($"Добавленная новая смена:\nНачало - {dateWorkStart}\nКонец - {dateWorkEnd}\nСтавка - {settingsBtn_WorkingRate.Text}", "Успешная операция");

                settingsBtn_dateWorking.SelectedDate = null;
                settingsBtn_startDate.Text = null;
                settingsBtn_endDate.Text = null;
                settingsBtn_WorkingRate = null;
            };
        }

        private void setEmpToTable(object sender, RoutedEventArgs e)
        {
            using(var dbContext = new PostgresContext())
            {
                WorkersShift workersShift = new WorkersShift();

                WorkersShift newString = new WorkersShift() 
                {
                    // Дописать логику!
                };
            };
        }

        private void openSettingsWindow(object sender, RoutedEventArgs e)
        {
            //try
            //{
            using (var dbContext = new PostgresContext())
            {
                AdminPanel.Visibility = Visibility.Hidden;
                settingsBtn.Visibility = Visibility.Visible;

                setEmptoWorkShifr_WorkShift.ItemsSource = dbContext.TypeWorksShifts
                 .Select(x => $"{ConvertTimeWithoutTimeZone(Convert.ToString(x.Start))} - {ConvertTimeWithoutTimeZone(Convert.ToString(x.End))}: {x.WorkingRate}")
                 .ToList();

                setEmptoWorkShifr_Employee.ItemsSource = dbContext.Users
                    .Select(x => $"{x.SecondName} {x.FirstName} {x.MiddleName}")
                    .ToList();

                //}
                //catch (Exception ex)
                //{
                //    // Обработайте ошибку, например, выведите ее в консоль или воспользуйтесь MessageBox
                //    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка");
                //}
            }
        }

        public static DateTime ConvertTimeWithoutTimeZone(string timeWithoutTimeZone)
        {
            DateTime resultDateTime;

            if (DateTime.TryParse(timeWithoutTimeZone, out resultDateTime))
            {
                // Если формат строки правильный, то возвращаем преобразованный объект DateTime
                return resultDateTime;
            }
            else
            {
                // Если формат строки неправильный, можно обработать ошибку или вернуть значение по умолчанию
                Console.WriteLine("Неверный формат времени.");
                return DateTime.MinValue;
            }
        }
    }
}
