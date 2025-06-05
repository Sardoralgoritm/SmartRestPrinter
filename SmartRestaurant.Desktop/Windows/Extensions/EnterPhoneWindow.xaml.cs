using SmartRestaurant.Desktop.Windows.BlurWindow;
using System.Windows;
using System.Windows.Input;

namespace SmartRestaurant.Desktop.Windows.Extensions;

/// <summary>
/// Interaction logic for EnterPhoneWindow.xaml
/// </summary>
public partial class EnterPhoneWindow : Window
{
    public string PhoneNumber { get; private set; } = string.Empty;

    public EnterPhoneWindow()
    {
        InitializeComponent();
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        string phone = txtPhoneNumber.Text.Trim();

        if (!phone.StartsWith("+998") || phone.Length != 13)
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Warning,
                "Telefon raqamni to‘g‘ri kiriting. Masalan: +998901234567",
                NotificationWindow.NotificationPosition.TopCenter);
            return;
        }

        string phoneNumberPart = phone.Substring(4);

        if (!phoneNumberPart.All(char.IsDigit))
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Warning,
                "Telefon raqamda faqat raqamlar bo‘lishi kerak.",
                NotificationWindow.NotificationPosition.TopCenter);
            return;
        }

        PhoneNumber = phone;
        this.DialogResult = true;
        this.Close();
    }

    private void Close_button_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);
        txtPhoneNumber.Text = "+998";
        txtPhoneNumber.CaretIndex = txtPhoneNumber.Text.Length;
        txtPhoneNumber.Focus();
    }

    private void txtPhoneNumber_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ConfirmButton_Click(null!, null!);
        }
    }
}
