using SmartRestaurant.Desktop.Helpers.Session;
using SmartRestaurant.Desktop.Pages;
using SmartRestaurant.Desktop.Windows.Auth;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private RadioButton? _lastCheckedNavButton;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var mainPos = new MainPOSPage();
        MainFrame.Content = mainPos;

        _lastCheckedNavButton = MainPOS_Button;
        MainPOS_Button.IsChecked = true;
    }

    private bool CheckAndConfirmBeforeNavigate()
    {
        if (MainFrame.Content is MainPOSPage mainPOS && mainPOS.HasPendingOrder())
        {
            return MessageBoxManager.ShowConfirmation("Buyurtma hali yakunlanmagan. Davom etsangiz, hozirgi buyurtma o‘chiriladi. Davom etasizmi?");
        }

        return true;
    }

    private void HandleNavigation(RadioButton currentButton, Page newPage)
    {
        if (!CheckAndConfirmBeforeNavigate())
        {
            if (_lastCheckedNavButton != null)
                _lastCheckedNavButton.IsChecked = true;

            currentButton.IsChecked = false;
            return;
        }

        MainFrame.Content = newPage;
        _lastCheckedNavButton = currentButton;
    }

    private void MainPOS_Click(object sender, RoutedEventArgs e)
    {
        HandleNavigation(MainPOS_Button, new MainPOSPage());
    }

    private void Menu_Click(object sender, RoutedEventArgs e)
    {
        HandleNavigation(Menu_Button, new MenuManagementPage());
    }

    private void Tables_Click(object sender, RoutedEventArgs e)
    {
        HandleNavigation(Tables_Button, new TablePage());
    }

    private void Report_Click(object sender, RoutedEventArgs e)
    {
        HandleNavigation(Report_Button, new ReportPage());
    }

    private void User_Button_Click(object sender, RoutedEventArgs e)
    {
        HandleNavigation(Users_Button, new UserPage());
    }

    private void Setting_Button_Click(object sender, RoutedEventArgs e)
    {
        HandleNavigation(Setting_Button, new SettingsPage());
    }

    private void Minus_Button_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        if (MainFrame.Content is MainPOSPage mainPOS && mainPOS.HasPendingOrder())
        {
            if(MessageBoxManager.ShowConfirmation("Buyurtma hali yakunlanmagan. Davom etsangiz, hozirgi buyurtma o‘chiriladi. Davom etasizmi?"))
            {
                Application.Current.Shutdown();
            }
        }
        else
        {
            Application.Current.Shutdown();
        }
    }
}
