using SmartRestaurant.Desktop.Pages;
using SmartRestaurant.Desktop.Windows.BlurWindow;
using SmartRestaurant.Desktop.Windows.Orders;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static SmartRestaurant.Desktop.Windows.Extensions.NotificationWindow;

namespace SmartRestaurant.Desktop.Windows.Extensions;

/// <summary>
/// Interaction logic for ColculatorWindow.xaml
/// </summary>
public partial class ColculatorWindow : Window
{
    public double OrderPrice;
    public event EventHandler<string?>? TakenAwayChanged;
    public event EventHandler? CheckPrinted;

    bool _isCorrect = false;
    public ColculatorWindow()
    {
        InitializeComponent();
    }

    private string FormatPrice(double price)
    {
        string priceStr = ((int)price).ToString();
        int length = priceStr.Length;

        for (int i = length - 3; i > 0; i -= 3)
        {
            priceStr = priceStr.Insert(i, " ");
        }

        return priceStr;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);
        txt_TotalPrice.Text = FormatPrice(OrderPrice);
    }

    private void Close_Button_Click(object sender, RoutedEventArgs e)
    {
        this.Close(); 
    }

    private void Save_Button_Click(object sender, RoutedEventArgs e)
    {
        ColculatePrice();

        if (txt_Payment_Price.Text.Length == 0)
        {
            foreach (Window window in Application.Current.Windows)
            {
                var frame = window.FindName("MainFrame") as Frame;
                if (frame != null && frame.Content is MainPOSPage salePage)
                {
                    salePage.PaymentPrice = OrderPrice;
                    break;
                }
            }

            TakenAwayChanged?.Invoke(this, null);

            this.Close();
        }
        else if (txt_Payment_Price.Text.Length > 0 && _isCorrect)
        {
            foreach (Window window in Application.Current.Windows)
            {
                var frame = window.FindName("MainFrame") as Frame;
                if (frame != null && frame.Content is MainPOSPage salePage)
                {
                    salePage.PaymentPrice = double.Parse(txt_Payment_Price.Text);
                    break;
                }
            }

            TakenAwayChanged?.Invoke(this, null);

            this.Close();
        }
        else
        {
            var sum = FormatPrice(OrderPrice - double.Parse(txt_Payment_Price.Text));
            NotificationManager.ShowNotification(MessageType.Error, $"Hisobga  {sum}  yetarli emas.");
        }
        _isCorrect = false;
    }

    private void txt_Total_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsDigit);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        txt_Payment_Price.Text = txt_Payment_Price.Text + button.Content.ToString();
    }

    private void Clear_Button_Click(object sender, RoutedEventArgs e)
    {
        if (txt_Payment_Price.Text.Length > 0)
        {
            txt_Payment_Price.Text = txt_Payment_Price.Text.Substring(0, txt_Payment_Price.Text.Length - 1);
        }
    }

    private void Colculate_Button_Click(object sender, RoutedEventArgs e)
    {
        txt_Refund_Price.Text = "";
        if (string.IsNullOrEmpty(txt_Payment_Price.Text))
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos to'lov miqdorini kiriting");
            return;
        }

        double paymentSum = double.Parse(txt_Payment_Price.Text);
        if (OrderPrice <= paymentSum)
        {
            txt_Refund_Price.Text = FormatPrice(paymentSum - OrderPrice);
            _isCorrect = true;
        }
        else
        {
            _isCorrect= false;
            lb_Quantity.Foreground = Brushes.Red;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 5,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(2)
            };

            var transform = new TranslateTransform();
            lb_Quantity.RenderTransform = transform;

            transform.BeginAnimation(TranslateTransform.XProperty, animation);

            var colorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            colorTimer.Tick += (s, e) =>
            {
                lb_Quantity.Foreground = Brushes.White;
                colorTimer.Stop();
            };

            colorTimer.Start();
        }
    }

    private void txt_Payment_Price_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true; 
        }
    }

    private void ColculatePrice()
    {
        double paymentSum = string.IsNullOrWhiteSpace(txt_Payment_Price.Text) ? 0 : double.Parse(txt_Payment_Price.Text);

        if (OrderPrice <= paymentSum)
        {
            txt_Refund_Price.Text = FormatPrice(paymentSum - OrderPrice);
            _isCorrect = true;
        }
        else
            _isCorrect = false;
    }

    private void btn_free_Click(object sender, RoutedEventArgs e)
    {
        var cancelWindow = new CancelOrderWindow();
        cancelWindow.windowTitle.Text = "Mahsulotni tekinga berish";
        cancelWindow.description.Text = "Mahsulotni tekinga berish uchun rahbardan parol talab qilinadi!";
        var result = cancelWindow.ShowDialog();

        if (result == true)
        {
            TakenAwayChanged?.Invoke(this, cancelWindow.Password);
        }
    }

    private void btn_check_Click(object sender, RoutedEventArgs e)
    {
        CheckPrinted?.Invoke(this, EventArgs.Empty);
    }
}
