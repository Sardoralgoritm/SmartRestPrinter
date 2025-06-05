using SmartRestaurant.Desktop.Windows.BlurWindow;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Windows;
using System.Windows.Input;

namespace SmartRestaurant.Desktop.Windows.Orders;

/// <summary>
/// Interaction logic for CancelOrderWindow.xaml
/// </summary>
public partial class CancelOrderWindow : Window
{
    public string Password = string.Empty;
    public CancelOrderWindow()
    {
        InitializeComponent();
    }

    private void Close_button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Password = pbPassword.Visibility == Visibility.Visible ? pbPassword.Password : tbPassword.Text;
        if (string.IsNullOrEmpty(Password))
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos parolni kiriting!");
        }
        else
        {
            this.DialogResult = true;
            this.Close();
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);
        pbPassword.Focus();
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

    private void btnDisVisible_Click(object sender, RoutedEventArgs e)
    {
        string password = tbPassword.Text;
        pbPassword.Password = password;
        tbPassword.Visibility = Visibility.Collapsed;
        pbPassword.Visibility = Visibility.Visible;
        btnVisible.Visibility = Visibility.Visible;
        btnDisVisible.Visibility = Visibility.Collapsed;
    }

    private void txtPhoneNumber_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Save_Click(null!, null!);
        }
    }
}
