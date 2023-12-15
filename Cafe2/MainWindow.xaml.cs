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
using OfficeOpenXml;
using OfficeOpenXml.Style;
using static Cafe2.MainWindow;
using System.Text;
using System.Windows.Xps.Packaging;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Fonts;
using PdfSharpCore.Utils;

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

        public class OrderReport
        {
            public DateTime DateOrder { get; set; }
            public string TableName { get; set; }
            public string ReadyStatus { get; set; }
            public string PaymentStatus { get; set; }
            public string PaymentMethod { get; set; }
            public List<DishReport> Dishes { get; set; }
            public decimal TotalOrderPrice { get; set; }
        }

        public class DishReport
        {
            public string NameDish { get; set; }
            public int Count { get; set; }
            public decimal DishPrice { get; set; }
            public decimal TotalDishPrice { get; set; }
        }

        public static int ordersId { get; set; }

        public static byte[] photoEmployee { get; set; }
        public static byte[] photoContract { get; set; }

        public static User selectEditUser { get; set; }

        public static List<OrderReport> orderReports { get; set; }
        public static List<OrderReport> orderReportsPaid { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _dbContext = new PostgresContext();
            GetGroupsList();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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

        public static System.Windows.Controls.Image ByteArrayToImage(byte[] imageBytes)
        {
            // Создаем объект BitmapImage
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(imageBytes);
            bitmapImage.EndInit();

            // Создаем объект Image
            System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image();
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
                DateTime dateWorkStart = (settingsBtn_dateWorking.SelectedDate ?? DateTime.UtcNow).Date;
                DateTime dateWorkEnd = (settingsBtn_dateWorking.SelectedDate ?? DateTime.UtcNow).Date;

                // Получаем выбранное время из ComboBox
                string startHour = settingsBtn_startDate.Text;
                string endHour = settingsBtn_endDate.Text;

                // Разбираем строку времени и добавляем к дате
                if (DateTime.TryParseExact(startHour, "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
                {
                    // Преобразование к UTC и добавление времени
                    dateWorkStart = dateWorkStart.Add(parsedTime.TimeOfDay);
                }

                if (DateTime.TryParseExact(endHour, "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime1))
                {
                    // Преобразование к UTC и добавление времени
                    dateWorkEnd = dateWorkEnd.Add(parsedTime1.TimeOfDay);
                }


                TypeWorksShift newType = new TypeWorksShift() 
                {
                    StartWorksShift = dateWorkStart.ToUniversalTime(),
                    EndWorksShift = dateWorkEnd.ToUniversalTime(),
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
            using (var dbContext = new PostgresContext())
            {
                WorkersShift workersShift = new WorkersShift();

                // Получаем пользователя с помощью AsEnumerable(), чтобы выполнить операцию на стороне клиента
                var idUser = dbContext.Users
                    .AsEnumerable()
                    .FirstOrDefault(x => $"{x.SecondName} {x.FirstName} {x.MiddleName}" == setEmptoWorkShifr_Employee.Text);

                var idTypeWorkShift = dbContext.TypeWorksShifts
                    .AsEnumerable()
                    .FirstOrDefault(x => $"{x.StartWorksShift} - {x.EndWorksShift}: {x.WorkingRate}" == setEmptoWorkShifr_WorkShift.Text);

                if (idUser != null && idTypeWorkShift != null && idTypeWorkShift.StartWorksShift > DateTime.UtcNow)
                {
                    WorkersShift newString = new WorkersShift()
                    {
                        Iduser = idUser.Iduser,
                        IdtypeWorkShift = idTypeWorkShift.IdtypeWorksShift
                    };
                    dbContext.WorkersShifts.Add(newString);
                    dbContext.SaveChanges();
                }
                else if (idUser == null)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка!");
                }
                else if (idTypeWorkShift == null)
                {
                    MessageBox.Show("Тип рабочей смены не найден.", "Ошибка!");
                }
                else
                {
                    MessageBox.Show("Эту смену нельзя выбрать!\nСмена просрочена", "Ошибка!");
                }
            }

        }

        private void openSettingsWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dbContext = new PostgresContext())
                {
                    AdminPanel.Visibility = Visibility.Hidden;
                    settingsBtn.Visibility = Visibility.Visible;

                    setEmptoWorkShifr_WorkShift.ItemsSource = dbContext.TypeWorksShifts
                        .Select(x => $"{x.StartWorksShift} - {x.EndWorksShift}: {x.WorkingRate}")
                        .ToList();

                    setEmptoWorkShifr_Employee.ItemsSource = dbContext.Users
                        .Select(x => $"{x.SecondName} {x.FirstName} {x.MiddleName}")
                        .ToList();

                    adddEmpToTable_ListEmp.ItemsSource = dbContext.Users
                        .Select(x => $"{x.SecondName} {x.FirstName} {x.MiddleName}")
                        .ToList();

                    adddEmpToTable_ListTables.ItemsSource = dbContext.Tables
                        .Select(x => x.NameTable)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                // Обработайте ошибку, например, выведите ее в консоль или воспользуйтесь MessageBox
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void adddEmpToTable_SetEmpToTable(object sender, RoutedEventArgs e)
        {
           using(var dbContext = new PostgresContext())
           {
                Table table = new Table();

                var idUser = dbContext.Users
                   .AsEnumerable()
                   .FirstOrDefault(x => $"{x.SecondName} {x.FirstName} {x.MiddleName}" == adddEmpToTable_ListEmp.Text);

                var idTable = dbContext.Tables
                    .AsEnumerable()
                    .FirstOrDefault(x => x.NameTable == adddEmpToTable_ListTables.Text);

                if ( idTable.UserTableId == null) 
                {
                    idTable.UserTableId = idUser.Iduser;
                    dbContext.SaveChanges();
                    MessageBox.Show($"Теперь у {idTable.NameTable} назначен отвечающий\nЗа стол отвечает {idUser.SecondName} {idUser.FirstName} {idUser.MiddleName}", "Назначен!");
                }
                else if (idTable.UserTableId != null)
                {
                    var oldUser = dbContext.Users
                        .FirstOrDefault(x => x.Iduser == idTable.UserTableId);
                    MessageBox.Show($"У {idTable.NameTable} изменен оффициант!" +
                        $"\nБыл: {oldUser.SecondName} {oldUser.FirstName} {oldUser.MiddleName}" +
                        $"\nТеперь: {idUser.SecondName} {idUser.FirstName} {idUser.MiddleName}","Изменен оффициант!");
                    idTable.UserTableId = idUser.Iduser;
                    dbContext.SaveChanges();
                }
            };
        }

        private void settingsBtn_backBtn(object sender, RoutedEventArgs e)
        {
            AdminPanel.Visibility = Visibility.Visible;
            settingsBtn.Visibility = Visibility.Hidden;
        }

        private void windowReports_backBtn(object sender, RoutedEventArgs e)
        {
            AdminPanel.Visibility = Visibility.Visible;
            windowReports.Visibility = Visibility.Hidden;
        }

        private void openWindowWithReports(object sender, RoutedEventArgs e)
        {
            AdminPanel.Visibility = Visibility.Hidden;
            windowReports.Visibility = Visibility.Visible;

            using(var dbContext = new PostgresContext()) 
            {
                var ordersWithDetailsAndTotalPrice = dbContext.Orders
                    .Include(order => order.DishInOrders)
                        .ThenInclude(dishInOrder => dishInOrder.IddishNavigation)
                    .Include(order => order.IdtableNumberNavigation)
                    .Include(order => order.IdreadyStatusNavigation)
                    .Include(order => order.IdpaymentStatusNavigation)
                    .Include(order => order.IdpaymentMethodNavigation)
                    .AsEnumerable() // Преобразуем IQueryable в IEnumerable
                    .Select(order => new
                    {
                        DateOrder = order.DateOrder,
                        TableName = order.IdtableNumberNavigation.NameTable,
                        ReadyStatus = order.IdreadyStatusNavigation.Name,
                        PaymentStatus = order.IdpaymentStatusNavigation.Name,
                        PaymentMethod = order.IdpaymentMethodNavigation.Name,
                        Dishes = order.DishInOrders.Select(dishInOrder => new
                        {
                            dishInOrder.IddishNavigation.NameDish,
                            dishInOrder.Count,
                            DishPrice = dishInOrder.IddishNavigation.Price,
                            TotalDishPrice = dishInOrder.Count * (decimal)dishInOrder.IddishNavigation.Price
                        }).ToList(),
                        TotalOrderPrice = order.DishInOrders.Sum(dishInOrder => dishInOrder.Count * (decimal)dishInOrder.IddishNavigation.Price)
                    })
                    .ToList();

                reportOrders.ItemsSource = ordersWithDetailsAndTotalPrice;
                orderReports = ordersWithDetailsAndTotalPrice
                    .Select(order => new OrderReport
                    {
                        DateOrder = order.DateOrder,
                        TableName = order.TableName,
                        ReadyStatus = order.ReadyStatus,
                        PaymentStatus = order.PaymentStatus,
                        PaymentMethod = order.PaymentMethod,
                        Dishes = order.Dishes.Select(dish => new DishReport
                        {
                            NameDish = dish.NameDish,
                            Count = dish.Count,
                            DishPrice = dish.DishPrice,
                            TotalDishPrice = dish.TotalDishPrice
                        }).ToList(),
                        TotalOrderPrice = order.TotalOrderPrice
                    })
                    .ToList();

                var paidOrders = dbContext.Orders
                    .Include(order => order.DishInOrders)
                        .ThenInclude(dishInOrder => dishInOrder.IddishNavigation)
                    .Include(order => order.IdtableNumberNavigation)
                    .Include(order => order.IdreadyStatusNavigation)
                    .Include(order => order.IdpaymentStatusNavigation)
                    .Include(order => order.IdpaymentMethodNavigation)
                    .Where(order => order.IdpaymentStatus == 1)
                    .AsEnumerable()
                    .Select(order => new OrderReport
                    {
                        DateOrder = order.DateOrder,
                        TableName = order.IdtableNumberNavigation.NameTable,
                        ReadyStatus = order.IdreadyStatusNavigation.Name,
                        PaymentStatus = order.IdpaymentStatusNavigation.Name,
                        PaymentMethod = order.IdpaymentMethodNavigation.Name,
                        Dishes = order.DishInOrders.Select(dishInOrder => new DishReport
                        {
                            NameDish = dishInOrder.IddishNavigation.NameDish,
                            Count = dishInOrder.Count,
                            DishPrice = dishInOrder.IddishNavigation.Price,
                            TotalDishPrice = dishInOrder.Count * (decimal)dishInOrder.IddishNavigation.Price
                        }).ToList(),
                        TotalOrderPrice = order.DishInOrders.Sum(dishInOrder => dishInOrder.Count * (decimal)dishInOrder.IddishNavigation.Price)
                    })
                    .ToList();

                reportOrdersTwo.ItemsSource = paidOrders;
                orderReportsPaid = paidOrders;

                // Рассчитываем итоговую стоимость всех заказов
                decimal totalPaidAmount = paidOrders.Sum(order => order.TotalOrderPrice);

                // Выводим итоговую стоимость в Label
                totalPricePaid.Content = $"Итоговая стоимость всех заказов: {totalPaidAmount:C}";
            };
        }

        private void exportReportOne(object sender, RoutedEventArgs e)
        {
            ExportToExcel(orderReports);
        }

        private void exportReportOne1(object sender, RoutedEventArgs e)
        {
            ExportToPdf(orderReports);
        }

        private void exportReportTwo(object sender, RoutedEventArgs e)
        {
            // Рассчитываем итоговую стоимость всех заказов
            decimal totalPaidAmount = orderReportsPaid.Sum(order => order.TotalOrderPrice);
            ExportToPdf(orderReportsPaid, totalPaidAmount);
        }

        private void exportReportTwo1(object sender, RoutedEventArgs e)
        {
            // Рассчитываем итоговую стоимость всех заказов
            decimal totalPaidAmount = orderReportsPaid.Sum(order => order.TotalOrderPrice);
            ExportToExcel(orderReportsPaid, totalPaidAmount);
        }

        private void ExportToExcel(List<OrderReport> data)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                // Заголовки столбцов
                string[] headers = { "Дата", "Столик", "Статус готовности", "Статус оплаты", "Метод оплаты", "Блюда", "Итоговая стоимость" };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                // Заполнение данными из списка OrderReport
                for (int row = 0; row < data.Count; row++)
                {
                    worksheet.Cells[row + 2, 1].Value = data[row].DateOrder;
                    worksheet.Cells[row + 2, 2].Value = data[row].TableName;
                    worksheet.Cells[row + 2, 3].Value = data[row].ReadyStatus;
                    worksheet.Cells[row + 2, 4].Value = data[row].PaymentStatus;
                    worksheet.Cells[row + 2, 5].Value = data[row].PaymentMethod;

                    // Заполнение данных для столбца "Блюда"
                    var dishes = data[row].Dishes;
                    StringBuilder dishesText = new StringBuilder();

                    foreach (var dish in dishes)
                    {
                        dishesText.AppendLine($"{dish.NameDish} - {dish.Count},");
                    }

                    worksheet.Cells[row + 2, 6].Value = dishesText.ToString().TrimEnd();

                    worksheet.Cells[row + 2, 7].Value = data[row].TotalOrderPrice;
                }

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx|All Files|*.*",
                    DefaultExt = ".xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var file = new FileInfo(saveFileDialog.FileName);
                    package.SaveAs(file);

                    MessageBox.Show("Отчет успешно экспортирован в Excel!", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ExportToPdf(List<OrderReport> data)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf|All Files|*.*",
                Title = "Выберите место сохранения PDF-отчета"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Загружаем встроенные шрифты PdfSharpCore
                GlobalFontSettings.FontResolver = new FontResolver();

                using (var document = new PdfDocument())
                {
                    var page = document.AddPage();
                    var graphics = XGraphics.FromPdfPage(page);
                    var font = new XFont("Arial", 10, XFontStyle.Regular);

                    var yPosition = 10;

                    foreach (var order in data)
                    {
                        if (yPosition + 80 > page.Height)
                        {
                            page = document.AddPage();
                            graphics = XGraphics.FromPdfPage(page);
                            yPosition = 10;
                        }

                        graphics.DrawString($"Дата: {order.DateOrder.ToString("dd.MM.yyyy HH:mm:ss")}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 15;

                        graphics.DrawString($"Столик: {order.TableName}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 15;

                        graphics.DrawString($"Статус готовности: {order.ReadyStatus}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 15;

                        graphics.DrawString($"Статус оплаты: {order.PaymentStatus}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 15;

                        graphics.DrawString($"Метод оплаты: {order.PaymentMethod}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 15;

                        var dishes = order.Dishes;
                        StringBuilder dishesText = new StringBuilder();
                        foreach (var dish in dishes)
                        {
                            dishesText.AppendLine($"{dish.NameDish} - {dish.Count}");
                        }

                        graphics.DrawString($"Блюда:\n{dishesText}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 20;

                        graphics.DrawString($"Итоговая стоимость: {order.TotalOrderPrice:C}", font, XBrushes.Black, 10, yPosition);

                        yPosition += 40;
                    }

                    document.Save(saveFileDialog.FileName);
                    MessageBox.Show($"PDF-отчет успешно выгружен в {saveFileDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ExportToExcel(List<OrderReport> data, decimal totalPaidAmount)
        {
            // Создаем диалоговое окно для выбора места сохранения файла
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx|All Files|*.*",
                Title = "Выберите место сохранения отчета"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("PaidOrdersReport");

                    string[] headers = { "Дата", "Столик", "Статус готовности", "Статус оплаты", "Метод оплаты", "Блюда", "Итоговая стоимость" };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    }

                    for (int row = 0; row < data.Count; row++)
                    {
                        worksheet.Cells[row + 2, 1].Value = data[row].DateOrder;
                        worksheet.Cells[row + 2, 2].Value = data[row].TableName;
                        worksheet.Cells[row + 2, 3].Value = data[row].ReadyStatus;
                        worksheet.Cells[row + 2, 4].Value = data[row].PaymentStatus;
                        worksheet.Cells[row + 2, 5].Value = data[row].PaymentMethod;

                        var dishes = data[row].Dishes;
                        StringBuilder dishesText = new StringBuilder();

                        foreach (var dish in dishes)
                        {
                            dishesText.AppendLine($"{dish.NameDish} - {dish.Count}");
                        }

                        worksheet.Cells[row + 2, 6].Value = dishesText.ToString().TrimEnd();

                        worksheet.Cells[row + 2, 7].Value = data[row].TotalOrderPrice;
                    }

                    worksheet.Cells[data.Count + 2, 6].Value = "Итоговая стоимость всех заказов:";
                    worksheet.Cells[data.Count + 2, 7].Value = totalPaidAmount;

                    // Извлекаем директорию и имя файла из пути, выбранного пользователем
                    var fileInfo = new FileInfo(saveFileDialog.FileName);

                    // Добавляем дату в имя файла
                    var fileNameWithDate = $"{fileInfo.DirectoryName}\\PaidOrdersReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    // Сохраняем файл
                    package.SaveAs(new FileInfo(fileNameWithDate));

                    MessageBox.Show($"Отчет успешно выгружен в {fileNameWithDate}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        // Метод для выгрузки в PDF
        private void ExportToPdf(List<OrderReport> data, decimal totalPaidAmount)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf|All Files|*.*",
                Title = "Выберите место сохранения PDF-отчета"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Загружаем встроенные шрифты PdfSharpCore
                GlobalFontSettings.FontResolver = new FontResolver();

                using (var document = new PdfDocument())
                {
                    // Создаем новую страницу
                    var page = document.AddPage();
                    var graphics = XGraphics.FromPdfPage(page);
                    var font = new XFont("Arial", 10, XFontStyle.Regular);

                    // Начинаем печать заголовков столбцов
                    var yPosition = 10;

                    // Для каждого заказа
                    foreach (var order in data)
                    {
                        // Проверяем, помещается ли информация о заказе на текущей странице
                        if (yPosition + 80 > page.Height) // 80 - примерная высота данных о заказе
                        {
                            // Если не помещается, создаем новую страницу
                            page = document.AddPage();
                            graphics = XGraphics.FromPdfPage(page);
                            yPosition = 10; // Сбрасываем позицию на новой странице
                        }

                        graphics.DrawString($"Дата: {order.DateOrder.ToString("dd.MM.yyyy HH:mm:ss")}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 15;

                        graphics.DrawString($"Столик: {order.TableName}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 15;

                        graphics.DrawString($"Статус готовности: {order.ReadyStatus}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 15;

                        graphics.DrawString($"Статус оплаты: {order.PaymentStatus}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 15;

                        graphics.DrawString($"Метод оплаты: {order.PaymentMethod}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 15;

                        var dishes = order.Dishes;
                        StringBuilder dishesText = new StringBuilder();
                        foreach (var dish in dishes)
                        {
                            dishesText.AppendLine($"{dish.NameDish} - {dish.Count}");
                        }

                        graphics.DrawString($"Блюда:\n{dishesText}", font, XBrushes.Black, 10, yPosition);
                        yPosition += 20;

                        // После каждого заказа добавляем отступ
                        yPosition += 40;
                    }

                    // Страница с итоговой стоимостью
                    var totalPage = document.AddPage();
                    var totalGraphics = XGraphics.FromPdfPage(totalPage);
                    var totalFont = new XFont("Arial", 10, XFontStyle.Bold);

                    var totalYPosition = 10;
                    totalGraphics.DrawString($"Итоговая стоимость всех заказов: {totalPaidAmount:C}", totalFont, XBrushes.Black, 10, totalYPosition);

                    // Сохраняем PDF-файл
                    document.Save(saveFileDialog.FileName);
                    MessageBox.Show($"PDF-отчет успешно выгружен в {saveFileDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }


    }
}
