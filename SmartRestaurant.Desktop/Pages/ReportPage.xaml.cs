using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SmartRestaurant.BusinessLogic.Services;
using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;
using SmartRestaurant.BusinessLogic.Services.Orders.Concrete;
using SmartRestaurant.BusinessLogic.Services.Orders.DTOs;
using SmartRestaurant.BusinessLogic.Services.Users.Concrete;
using SmartRestaurant.Desktop.Components;
using SmartRestaurant.Desktop.Components.Product;
using SmartRestaurant.Desktop.Helpers.Session;
using SmartRestaurant.Desktop.Windows.Extensions;
using SmartRestaurant.Domain.Const;
using SmartRestaurant.Domain.Models.PageResult;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartRestaurant.Desktop.Pages;

/// <summary>
/// Interaction logic for ReportPage.xaml
/// </summary>
public partial class ReportPage : Page, INotifyPropertyChanged
{
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    public event PropertyChangedEventHandler? PropertyChanged;
    int _currentPage = 1;
    int _lastPage = 1;
    private string? _filterBy;
    private string? _status;
    private Guid _userId;
    public ReportPage()
    {
        InitializeComponent();
        DataContext = this;
        _orderService = App.ServiceProvider.GetRequiredService<IOrderService>();
        _userService = App.ServiceProvider.GetRequiredService<IUserService>();
    }

    private IEnumerable<ISeries>? _series;
    public IEnumerable<ISeries> Series
    {
        get => _series!;
        set
        {
            _series = value;
            OnPropertyChanged(nameof(Series));
        }
    }

    private IEnumerable<Axis>? _xAxes;
    public IEnumerable<Axis> XAxes
    {
        get => _xAxes!;
        set
        {
            _xAxes = value;
            OnPropertyChanged(nameof(XAxes));
        }
    }

    private IEnumerable<Axis>? _yAxes;
    public IEnumerable<Axis> YAxes
    {
        get => _yAxes!;
        set
        {
            _yAxes = value;
            OnPropertyChanged(nameof(YAxes));
        }
    }

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void LoadChartData(List<OrderItemDto> dto)
    {
        var productNames = dto.Select(d => d.ProductName).ToList();
        var soldCounts = dto.Select(d => d.Quantity).ToList();

        Series = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = soldCounts,
                Fill = new SolidColorPaint(SKColors.Orange)
            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                Labels = productNames.Select(name => name.Length > 12 ? name.Substring(0, 12) + "..." : name).ToArray(),
                LabelsPaint = new SolidColorPaint(SKColors.Black),
                TextSize = 12
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(SKColors.Black),
                TextSize = 16
            }
        };
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

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await GetReport();

        var users = await _userService.GetAllUsersAsync();
        cbUsers.ItemsSource = users;
        cbUsers.SelectedValuePath = "Id";

        foreach (var scrollViewer in FindVisualChildren<ScrollViewer>(this))
        {
            scrollViewer.ManipulationBoundaryFeedback += (s, args) =>
            {
                args.Handled = true;
            };
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
            {
                yield return t;
            }
            foreach (var childOfChild in FindVisualChildren<T>(child))
            {
                yield return childOfChild;
            }
        }
    }

    public async Task GetReport()
    {
        st_Order.Children.Clear();
        EmptyData.Visibility = Visibility.Collapsed;
        Loader.Visibility = Visibility.Visible;

        var option = new OrderSortFilterOptions()
        {
            Status = OrderStatus.Closed,
            SortBy = "queue"
        };

        if (fromDateTime.SelectedDate != null && toDateTime.SelectedDate != null)
        {
            option.FromDateTime = fromDateTime.SelectedDate.Value;
            option.ToDateTime = toDateTime.SelectedDate.Value;
        }  

        option.Page = _currentPage;

        var orders = _orderService.GetAll(option);

        _lastPage = orders.TotalPages;
        Last_Page_Button.Content = orders.TotalPages;

        var report = await _orderService.GetReportOrder(option);

        ShowOrders(orders);
        ShowReport(report);
    }

    public void ShowOrders(PagedResult<OrderDto> dto)
    {
        Loader.Visibility = Visibility.Collapsed;
        if(dto.Items.Count > 0)
        {
            EmptyData.Visibility = Visibility.Collapsed;
            int count = (dto.Page - 1) * dto.PageSize;

            foreach (var order in dto.Items)
            {
                OrderComponent orderComponent = new OrderComponent();
                orderComponent.SetData(order, ++count);
                orderComponent.OrderCanceled += async (s, e) =>
                {
                    var result = await _orderService.CancelOrderAsync(order.Id, SessionManager.CurrentUserId);
                    if (result)
                    {
                        await GetReport();
                    }
                };
                orderComponent.Tag = order;
                st_Order.Children.Add(orderComponent);
            }
        }
        else
        {
            EmptyData.Visibility = Visibility.Visible;
        }
    }

    private void ShowProducts(PagedResult<OrderItemDto> dto)
    {
        Loader.Visibility = Visibility.Collapsed;
        if (dto.Items.Count > 0)
        {
            EmptyData.Visibility = Visibility.Collapsed;
            int count = (dto.Page - 1) * dto.PageSize;

            foreach (var product in dto.Items)
            {
                ProductReport productComponent = new ProductReport();
                productComponent.SeedData(product, ++count);
                st_Order.Children.Add(productComponent);
            }
        }
        else
        {
            EmptyData.Visibility = Visibility.Visible;
        }
    }

    public void ShowReport(ReportOrderDto dto)
    {
        lb_TotalPrice.Content = FormatPrice(dto.TotalRevenue);
        lb_OrderCount.Content = dto.TotalOrdersCount;
        if (dto.TotalOrdersCount > 0)
        {
            LoadChartData(dto.TopOrderItems);
            ChartEmptyData.Visibility = Visibility.Collapsed;
        }
        else
        {
            ChartEmptyData.Visibility = Visibility.Visible;
            Series = Array.Empty<ISeries>();
        }
    }

    private async void Pervious_Button_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage -= 1;
            txt_pageNumber.Text = _currentPage.ToString();
        }

        await GetReport();
    }

    private async void Next_Button_Click(object sender, RoutedEventArgs e)
    {
        if (_lastPage > _currentPage)
        {
            _currentPage += 1;
            txt_pageNumber.Text = _currentPage.ToString();
        }
        await GetReport();
    }

    private async void Last_Page_Button_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage != _lastPage)
        {
            _currentPage = _lastPage;
            txt_pageNumber.Text = _currentPage.ToString();
        }
        await GetReport();
    }

    private async void txt_pageNumber_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (!string.IsNullOrWhiteSpace(txt_pageNumber.Text))
            {
                var currentPage = int.Parse(txt_pageNumber.Text);
                if (currentPage > _lastPage)
                {
                    _currentPage = _lastPage;
                    txt_pageNumber.Text = _lastPage.ToString();
                }
                else
                    _currentPage = currentPage;
                await GetReport();
            }
        }
    }

    private void txt_pageNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
    }

    private async void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        st_Order.Children.Clear();
        if (status.SelectedIndex == 3)
        {
            AdaptGridForProductReport();

            var option = new OrderSortFilterOptions();

            if (fromDateTime.SelectedDate != null && toDateTime.SelectedDate != null)
            {
                option.FromDateTime = fromDateTime.SelectedDate.Value;
                option.ToDateTime = toDateTime.SelectedDate.Value;
            }
            else if (fromDateTime.SelectedDate != null || toDateTime.SelectedDate != null)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos, boshlanish va tugash vaqtini to'liq kiriting!", NotificationWindow.NotificationPosition.TopCenter);
                return;
            }

            option.Page = _currentPage;

            var products = _orderService.GetAllReportOrder(option);
            var report = await _orderService.GetReportOrder(option);
            ShowProducts(products);
            ShowReport(report);
            return;
        }
        else
        {
            RestoreToOriginalState();
        }

        if (status.SelectedIndex != -1)
        {
            var option = new OrderSortFilterOptions
            {
                Status = _status,
                SortBy = "queue"
            };

            if (fromDateTime.SelectedDate != null && toDateTime.SelectedDate != null)
            {
                option.FromDateTime = fromDateTime.SelectedDate.Value;
                option.ToDateTime = toDateTime.SelectedDate.Value;
            }
            else if (fromDateTime.SelectedDate != null || toDateTime.SelectedDate != null)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos, boshlanish va tugash vaqtini to'liq kiriting!", NotificationWindow.NotificationPosition.TopCenter);
                return;
            }

            option.Page = _currentPage;

            var orders = _orderService.GetAll(option);

            _lastPage = orders.TotalPages;
            Last_Page_Button.Content = orders.TotalPages;

            var report = await _orderService.GetReportOrder(option);

            ShowOrders(orders);
            ShowReport(report);
        }
        else
        {
            if (cbUsers.SelectedIndex == -1)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos, foydalanuvchini tanlang!", NotificationWindow.NotificationPosition.TopCenter);
                return;
            }

            if (filterBy.SelectedIndex == -1)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos, filterni tanlang!", NotificationWindow.NotificationPosition.TopCenter);
                return;
            }

            var option = new OrderSortFilterOptions
            {
                SortBy = "queue"
            };

            if (_filterBy == "Ordered")
            {
                option.OrderedUserId = _userId;
            }
            else
            {
                option.ClosedUserId = _userId;
            }

            if (fromDateTime.SelectedDate != null && toDateTime.SelectedDate != null)
            {
                option.FromDateTime = fromDateTime.SelectedDate.Value;
                option.ToDateTime = toDateTime.SelectedDate.Value;
            }

            option.Page = _currentPage;

            var orders = _orderService.GetAll(option);

            _lastPage = orders.TotalPages;
            Last_Page_Button.Content = orders.TotalPages;

            var report = await _orderService.GetReportOrder(option);

            ShowOrders(orders);
            ShowReport(report);
        }
    }

    private void cbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cbUsers.SelectedIndex != -1)
        {
            if (status != null)
                status.SelectedIndex = -1;

            _userId = (Guid)cbUsers.SelectedValue;
        }
    }

    private void status_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (status.SelectedIndex != -1)
        {
            if (cbUsers != null)
                cbUsers.SelectedIndex = -1;

            if (filterBy != null)
                filterBy.SelectedIndex = -1;


            if (status.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedText = selectedItem.Content.ToString();

                switch (selectedText)
                {
                    case "Bekor qilingan":
                        _status = OrderStatus.Canceled;
                        break;
                    case "Tekinga berilgan":
                        _status = OrderStatus.Free;
                        break;
                    case "Yopilgan":
                        _status = OrderStatus.Closed;
                        break;
                    default:
                        _status = OrderStatus.Closed;
                        break;
                }
            }
        }
    }

    private void filterBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (filterBy.SelectedIndex != -1)
        {
            if (status != null)
                status.SelectedIndex = -1;


            if (filterBy.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedText = selectedItem.Content.ToString();

                switch (selectedText)
                {
                    case "Xizmat ko'rsatish":
                        _filterBy = "Ordered";
                        break;
                    case "Kassaga oid":
                        _filterBy = "Closed";
                        break;
                    default:
                        _filterBy = "Ordered";
                        break;
                }
            }
        }
    }

    private void AdaptGridForProductReport()
    {
        Report_name.ColumnDefinitions.Clear();

        Report_name.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
        Report_name.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
        Report_name.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Report_name.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });

        UpdateHeaderTexts();
    }

    private void UpdateHeaderTexts()
    {
        // Mavjud TextBlock larni topish va yangilash
        foreach (UIElement child in Report_name.Children)
        {
            if (child is TextBlock textBlock)
            {
                int column = Grid.GetColumn(textBlock);
                switch (column)
                {
                    case 0:
                        textBlock.Text = "No";
                        break;
                    case 1:
                        textBlock.Text = "Mahsulot nomi";
                        break;
                    case 2:
                        textBlock.Text = "Soni";
                        break;
                    case 3:
                        textBlock.Text = "Jami summa";
                        break;
                    default:
                        textBlock.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }
    }

    private void RestoreToOriginalState()
    {
        try
        {
            RestoreOriginalColumns();

            RestoreOriginalHeaders();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error restoring grid: {ex.Message}");
        }
    }

    private void RestoreOriginalColumns()
    {
        Report_name.ColumnDefinitions.Clear();

        Report_name.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
        Report_name.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        Report_name.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Report_name.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Report_name.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Report_name.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
    }

    private void RestoreOriginalHeaders()
    {
        Report_name.Children.Clear();

        var originalHeaders = new string[] { "No", "Transaction id", "Stol nomi", "Manager", "Narxi" };

        for (int i = 0; i < originalHeaders.Length; i++)
        {
            var textBlock = new TextBlock
            {
                Text = originalHeaders[i],
                Style = (Style)FindResource("TitleLabel")
            };

            if (i == 0)
            {
                textBlock.Margin = new Thickness(10, 0, 0, 0);
            }

            Grid.SetColumn(textBlock, i);
            Report_name.Children.Add(textBlock);
        }
    }
}
