using System;
using System.Linq;
using System.Windows;

namespace WpfApp2
{
    public partial class LoginWindow : Window
    {
        private SportsAppEntities db = new SportsAppEntities();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void ShowRegistration_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            TitleText.Text = "РЕГИСТРАЦИЯ";
            errorBorder.Visibility = Visibility.Collapsed;
        }

        private void ShowLogin_Click(object sender, RoutedEventArgs e)
        {
            RegisterPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            TitleText.Text = "ВХОД В СИСТЕМУ";
            errorBorder.Visibility = Visibility.Collapsed;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingBorder.Visibility = Visibility.Visible;
                errorBorder.Visibility = Visibility.Collapsed;
                btnLogin.IsEnabled = false;

                string email = txtUsername.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrWhiteSpace(email))
                {
                    ShowError("Введите email");
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    ShowError("Введите пароль");
                    return;
                }

                var user = db.Users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    ShowError("Пользователь не найден");
                    return;
                }

                if (user.PasswordHash == password)
                {
                    MessageBox.Show($"Добро пожаловать, {user.Email}!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    MainWindow mainWindow = new MainWindow(user);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ShowError("Неверный пароль");
                }
            }
            catch (Exception ex)
            {
                ShowError("Ошибка: " + ex.Message);
            }
            finally
            {
                loadingBorder.Visibility = Visibility.Collapsed;
                btnLogin.IsEnabled = true;
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = txtRegEmail.Text.Trim();
                string password = txtRegPassword.Password;
                string confirmPassword = txtRegConfirmPassword.Password;

                if (string.IsNullOrWhiteSpace(email))
                {
                    ShowError("Введите email");
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    ShowError("Введите пароль");
                    return;
                }

                if (password != confirmPassword)
                {
                    ShowError("Пароли не совпадают");
                    return;
                }

                if (password.Length < 6)
                {
                    ShowError("Пароль должен содержать минимум 6 символов");
                    return;
                }

                var existingUser = db.Users.FirstOrDefault(u => u.Email == email);
                if (existingUser != null)
                {
                    ShowError("Пользователь с таким email уже существует");
                    return;
                }

                var newUser = new Users
                {
                    Email = email,
                    PasswordHash = password,
                    Role = "user",
                    CreatedAt = DateTime.Now
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Регистрация успешно завершена! Теперь вы можете войти.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                ShowLogin_Click(sender, e);

                txtRegEmail.Text = "";
                txtRegPassword.Password = "";
                txtRegConfirmPassword.Password = "";
            }
            catch (Exception ex)
            {
                ShowError("Ошибка регистрации: " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (db != null)
            {
                db.Dispose();
            }
            base.OnClosed(e);
        }
    }
}