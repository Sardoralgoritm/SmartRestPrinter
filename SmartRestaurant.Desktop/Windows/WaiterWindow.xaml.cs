using SmartRestaurant.Desktop.Pages;
using System.Windows;

namespace SmartRestaurant.Desktop.Windows;

/// <summary>
/// Interaction logic for WaiterWindow.xaml
/// </summary>
public partial class WaiterWindow : Window
{
    public WaiterWindow()
    {
        InitializeComponent();
    }

    private void MainPOS_Click(object sender, RoutedEventArgs e)
    {
        var mainPos = new MainPOSPage();
        MainFrame.Content = mainPos;
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
