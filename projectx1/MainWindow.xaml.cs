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

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Пожалуйста, введите логин и пароль");
                    return;
                }

                using (var context = new projectxEntities())
                {
                    var user = await context.Users
                        .Where(u => u.username == username)
                        .FirstOrDefaultAsync();

                if (username.ToLower() == "admin" && password == "admin")
                {
                    MessageBox.Show("Вы успешно авторизовались как администратор", "Добро пожаловать!",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    Admin adminWindow = new Admin();
                    adminWindow.Show();
                    this.Close();
                    return;
                }
                if (user == null)
                    {
                        MessageBox.Show("Неправильный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Проверяем, есть ли свойство IsLocked и заблокирован ли пользователь
                    if (user.IsLocked == true)
                    {
                        MessageBox.Show("Вы заблокированы, обратитесь, пожалуйста к администратору.", "Доступ запрещен.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Проверяем длительное отсутствие (если есть LastLoginDate)
                    if (user.LastLoginDate.HasValue && (DateTime.Now - user.LastLoginDate.Value).TotalDays > 30 && user.role != "Admin")
                    {
                        user.IsLocked = true;
                        await context.SaveChangesAsync();
                        MessageBox.Show("Ваша учетная запись заблокирована из-за длительного отсутствия", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (user.password == password)
                    {
                        user.LastLoginDate = DateTime.Now;
                        user.FailedLoginAttempts = 0;
                        await context.SaveChangesAsync();

                        MessageBox.Show("Вы успешно авторизовались", "Добро пожаловать!", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (user.IsFirstLogin.HasValue && user.IsFirstLogin.Value)
                    {
                        ChangePassword changePasswordWindow = new ChangePassword(user.id);
                        changePasswordWindow.Owner = this;
                        changePasswordWindow.ShowDialog();
                    }
                    else
                    {
                        if (user.role == "Admin")
                        {
                            Admin adminWindow = new Admin();
                            adminWindow.Show();
                        }
                        else
                        {
                            MainWindow userWindow = new MainWindow();
                            userWindow.Show();
                        }
                        this.Close();
                    }

                }
                else
                    {
                        user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;

                        if (user.FailedLoginAttempts >= 3)
                        {
                            user.IsLocked = true;
                            await context.SaveChangesAsync();
                            MessageBox.Show("неудача", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            int attemptsLeft = 3 - (user.FailedLoginAttempts ?? 0);
                            await context.SaveChangesAsync();
                            MessageBox.Show($"неудача. Осталось попыток: {attemptsLeft}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }
