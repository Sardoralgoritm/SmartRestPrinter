using SmartRestaurant.Desktop.Pages;
using System.Windows;

namespace SmartRestaurant.Desktop.Windows;

/// <summary>
/// Interaction logic for ManagerWindow.xaml
/// </summary>
public partial class ManagerWindow : Window
{
    public ManagerWindow()
    {
        InitializeComponent();
    }

    private void MainPOS_Click(object sender, RoutedEventArgs e)
    {
        var mainPos = new MainPOSPage();
        MainFrame.Content = mainPos;
    }

    private void Tables_Click(object sender, RoutedEventArgs e)
    {
        var tablePage = new TablePage();
        MainFrame.Content = tablePage;
    }

    private void Menu_Click(object sender, RoutedEventArgs e)
    {
        var menuManagementPage = new MenuManagementPage();
        MainFrame.Content = menuManagementPage;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var mainPos = new MainPOSPage();
        MainFrame.Content = mainPos;
    }

    private void Setting_Button_Click(object sender, RoutedEventArgs e)
    {
        SettingsPage settingsPage = new SettingsPage();
        MainFrame.Content = settingsPage;
    }

    private void Minus_Button_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }
}
