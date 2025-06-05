using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.BusinessLogic.Services.Auth.Concrete;
using SmartRestaurant.BusinessLogic.Services.Auth.DTOs;
using SmartRestaurant.Desktop.Helpers.Session;
using SmartRestaurant.Desktop.Windows.Extensions;
using SmartRestaurant.Domain.Const;
using System.Windows;
using System.Windows.Input;
using static SmartRestaurant.Desktop.Windows.Extensions.NotificationWindow;

namespace SmartRestaurant.Desktop.Windows.Auth
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        public LoginWindow()
        {
            InitializeComponent();
            _authService = App.ServiceProvider.GetRequiredService<IAuthService>();
        }

        private void btnDisVisible_Click(object sender, RoutedEventArgs e)
        {
            string password = tbPassword.Text;
            pbPassword.Password = password;
            tbPassword.Visibility = Visibility.Collapsed;
            pbPassword.Visibility = Visibility.Visible;
            btnVisible.Visibility = Visibility.Visible;
            btnDisVisible.Visibility = Visibility.Collapsed;
        }

        private async void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            Login_Button.Visibility = Visibility.Collapsed;
            Loader.Visibility = Visibility.Visible;

            string password = pbPassword.Visibility == Visibility.Visible ? pbPassword.Password : tbPassword.Text;

            var user = new UserLoginDto
            {
                Password = password
            };

            var isExist = await _authService.LoginAsync(user);

            if (isExist != null)
            {
                SessionManager.CurrentUserId = isExist.Id;
                SessionManager.FirstName = isExist.FirstName;
                SessionManager.LastName = isExist.LastName;
                SessionManager.Role = isExist.Role;

                if (isExist.Role == Roles.WAITER)
                {
                    WaiterWindow waiterWindow = new WaiterWindow();
                    waiterWindow.Show();
                    this.Close();
                }
                else if (isExist.Role == Roles.MANAGER)
                {
                    ManagerWindow managerWindow = new ManagerWindow();
                    managerWindow.Show();
                    this.Close();
                }
                else
                {
                    MainWindow window = new MainWindow();
                    window.Show();
                    this.Close();
                }
            }
            else
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Bunday foydalanuvchi mavjud emas!", NotificationPosition.TopCenter);
                Loader.Visibility = Visibility.Collapsed;
                Login_Button.Visibility = Visibility.Visible;
            }
        }

        private void btnVisible_Click(object sender, RoutedEventArgs e)
        {
            string password = pbPassword.Password;
            tbPassword.Text = password;
            tbPassword.Visibility = Visibility.Visible;
            pbPassword.Visibility = Visibility.Collapsed;
            btnVisible.Visibility = Visibility.Collapsed;
            btnDisVisible.Visibility = Visibility.Visible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            pbPassword.Focus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void txtPhoneNumber_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Button_Click(null!, null!);
            }
        }
    }
}
