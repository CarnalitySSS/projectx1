using System.Windows;
using System.Windows.Controls;

namespace projectx1
{
    public partial class AddUserWindow : Window
    {
        public Users NewUser { get; private set; }

        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewUser = new Users
            {
                username = txtUsername.Text,
                password = txtPassword.Password,
                role = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString(),
                IsFirstLogin = true,
                IsLocked = false
            };

            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}