using SmartRestaurant.BusinessLogic.Services.Users.DTOs;
using SmartRestaurant.Desktop.Windows.BlurWindow;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SmartRestaurant.Desktop.Windows.Users;

/// <summary>
/// Interaction logic for ViewUserWindow.xaml
/// </summary>
public partial class ViewUserWindow : Window
{
    private UserDto _user;
    public ViewUserWindow(UserDto user)
    {
        InitializeComponent();
        _user = user;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);

        txtFullName.Text = $"{_user.FirstName} {_user.LastName}";
        txtPhone.Text = $"📱 {_user.PhoneNumber}";
        txtRole.Text = $"👔 {_user.Role}";

        if (!string.IsNullOrWhiteSpace(_user.ImageUrl))
        {
            try
            {
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _user.ImageUrl);
                if (File.Exists(imagePath))
                {
                    userImage.Source = new BitmapImage(new Uri(imagePath));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Rasm yuklashda xatolik: " + ex.Message);
            }
        }
    }

    private void Close_Button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        userImage.Source = null;
    }
}
