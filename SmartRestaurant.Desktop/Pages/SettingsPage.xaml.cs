using SmartRestaurant.Desktop.Service;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;
using static SmartRestaurant.Desktop.Windows.Extensions.NotificationWindow;

namespace SmartRestaurant.Desktop.Pages;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = this;
    }
    private void LotteryToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        LotteryManager.IsLotteryModeEnabled = true;
        LotteryManager.SaveSettings();
    }

    private void LotteryToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        LotteryManager.IsLotteryModeEnabled = false;
        LotteryManager.SaveSettings();
    }

    private void cb_UserPrinter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(cb_UserPrinter.SelectedItem is ComboBoxItem item)
        {
            Properties.Settings.Default.UserPrinterName = item.Content.ToString();
            Properties.Settings.Default.Save();
        }
    }

    //private void cb_ChefPrinter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    if (cb_ChefPrinter.SelectedItem is ComboBoxItem item)
    //    {
    //        Properties.Settings.Default.ChefProntetName = item.Content.ToString();
    //        Properties.Settings.Default.Save();
    //    }
    //}

    public void GetPrinters()
    {
        //cb_ChefPrinter.Items.Clear();
        cb_UserPrinter.Items.Clear();

        bool found = false;

        foreach (string printerName in PrinterSettings.InstalledPrinters)
        {
            ComboBoxItem chefItem = new ComboBoxItem
            {
                Content = printerName,
                FontSize = 18
            };

            ComboBoxItem userItem = new ComboBoxItem
            {
                Content = printerName,
                FontSize = 18
            };

            //cb_ChefPrinter.Items.Add(chefItem);
            cb_UserPrinter.Items.Add(userItem);

            found = true;
        }

        if (!found)
        {
            ComboBoxItem notFound1 = new ComboBoxItem
            {
                Content = "Printer topilmadi",
                IsEnabled = false,
                FontSize = 18
            };

            ComboBoxItem notFound2 = new ComboBoxItem
            {
                Content = "Printer topilmadi",
                IsEnabled = false,
                FontSize = 18
            };

            //cb_ChefPrinter.Items.Add(notFound1);
            cb_UserPrinter.Items.Add(notFound2);
        }
    }

    private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        GetPrinters();
        LotteryToggleButton.IsChecked = LotteryManager.IsLotteryModeEnabled;
    }

    private void Save_Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if(txt_Phone_Number.Text.Length == 9)
        {
            Properties.Settings.Default.PhoneNumber = "+998" + txt_Phone_Number.Text;
            Properties.Settings.Default.Save();
            txt_Phone_Number.Text = "";
        }
        else
        {
            NotificationManager.ShowNotification(MessageType.Error, "Telefon raqam to'liq emas.");
        }
    }
}
