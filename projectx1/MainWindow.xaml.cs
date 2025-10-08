using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace projectx1
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

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль");
                return;
            }

            try
            {
                using (var context = new projectxEntities())
                {
                    // Сначала проверяем админа (без запроса к БД)
                    if (username.ToLower() == "admin" && password == "admin")
                    {
                        MessageBox.Show("Вы успешно авторизовались как администратор", "Добро пожаловать!",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        Admin adminWindow = new Admin();
                        adminWindow.Show();
                        this.Close();
                        return;
                    }
                    if (username.ToLower() == "user" && password == "user")
                    {
                        MessageBox.Show("Вы успешно авторизовались как пользователь", "Добро пожаловать!",  
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        UserWindow userWindow = new UserWindow();  
                        userWindow.Show();
                        this.Close();
                        return;
                    }

                    // Только если не админ - ищем пользователя в БД
                    var users = await context.Users
                        .Where(u => u.username == username)
                        .FirstOrDefaultAsync();

                    if (users == null)
                    {
                        MessageBox.Show("Неправильный логин или пароль.", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Проверяем, заблокирован ли пользователь
                    if (users.isLocked == true)
                    {
                        MessageBox.Show("Вы заблокированы, обратитесь к администратору.",
                            "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Проверяем длительное отсутствие
                    if (users.LastLoginDate.HasValue &&
                        (DateTime.Now - users.LastLoginDate.Value).TotalDays > 30 &&
                        users.role != "Admin")
                    {
                        users.isLocked = true;
                        await context.SaveChangesAsync();
                        MessageBox.Show("Ваша учетная запись заблокирована из-за длительного отсутствия",
                            "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Проверяем пароль
                    if (users.password == password)
                    {
                        // Успешный вход
                        users.LastLoginDate = DateTime.Now;
                        users.FailedLoginAttempts = 0;
                        await context.SaveChangesAsync();

                        MessageBox.Show("Вы успешно авторизовались", "Добро пожаловать!",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // Проверяем первый вход
                        if (users.IsFirstLogin.HasValue && users.IsFirstLogin.Value)
                        {
                            ChangePassword changePasswordWindow = new ChangePassword(users.id);
                            changePasswordWindow.Owner = this;
                            changePasswordWindow.ShowDialog();
                        }
                        else
                        {
                            // Открываем соответствующее окно по роли
                            if (users.role == "Admin")
                            {
                                Admin adminWindow = new Admin();
                                adminWindow.Show();
                            }
                            else
                            {
                                // Для обычного пользователя - создайте UserWindow или используйте другую форму
                                UserWindow userWindow = new UserWindow();
                                userWindow.Show();
                            }
                            this.Close();
                        }
                    }
                    else
                    {
                        // Неправильный пароль
                        users.FailedLoginAttempts = (users.FailedLoginAttempts +0) + 1;

                        if (users.FailedLoginAttempts >= 3)
                        {
                            users.isLocked = true;
                            await context.SaveChangesAsync();
                            MessageBox.Show("Слишком много неудачных попыток входа. Аккаунт заблокирован.",
                                "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            int attemptsLeft = 3 - (users.FailedLoginAttempts - 0);
                            await context.SaveChangesAsync();
                            MessageBox.Show($"Неправильный пароль. Осталось попыток: {attemptsLeft}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}