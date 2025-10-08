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
    public partial class Admin : Window
    {
        private void CheckAllEntities()
        {
            using (var context = new projectxEntities())
            {
                var props = context.GetType().GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                               p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .ToList();

                string message = "Все сущности в projectxEntities:\n";
                foreach (var prop in props)
                {
                    var entityType = prop.PropertyType.GetGenericArguments()[0];
                    message += $"- {prop.Name} (тип: {entityType.Name})\n";
                }
                MessageBox.Show(message);
            }
        }
        public Admin()
        {
            InitializeComponent(); ;
            LoadUsers();
            CheckAllEntities();
        }

        private async void LoadUsers()
        {
            try
            {
                using (var context = new projectxEntities())
                { 
                     var users = await context.Users.ToListAsync();
                     Users.ItemsSource = users;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }
        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var newUserWindow = new AddUserWindow();

            if (newUserWindow.ShowDialog() == true && newUserWindow.NewUser != null)
            {
                var newUser = newUserWindow.NewUser;

                using (var context = new projectxEntities())
                {
                    try
                    {
                        // Используйте то же название, что и в LoadUsers
                        bool userExists = await context.Users.AnyAsync(u => u.username == newUser.username);

                        if (userExists)
                        {
                            MessageBox.Show("Пользователь с таким логином уже существует");
                            return;
                        }

                        context.Users.Add(newUser);
                        await context.SaveChangesAsync();
                        MessageBox.Show("Пользователь добавлен!");

                        // Обновляем список
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
      
            else
            {
                MessageBox.Show("Добавление пользователя отменено.", "Отмена", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UnlockUser_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

