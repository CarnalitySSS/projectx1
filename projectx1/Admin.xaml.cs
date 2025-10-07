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
        public Admin()
        {
            InitializeComponent();
            LoadUsers();
        }

        private async void LoadUsers()
        {
            using (var context = new projectxEntities())
            {
                var users = await context.Users.ToListAsync();
                Users.ItemsSource = users;
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
                    if (await context.Users.AnyAsync(u => u.username == newUser.username))
                    {
                        MessageBox.Show("Пользователь с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        try
                        {
                            context.Users.Add(newUser);
                            await context.SaveChangesAsync();
                        }
                        catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                        {
                            MessageBox.Show($"Ошибка при сохранении данных: {ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        LoadUsers();
                        MessageBox.Show("Пользователь успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

