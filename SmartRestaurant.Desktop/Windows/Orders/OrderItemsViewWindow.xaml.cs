using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;
using SmartRestaurant.BusinessLogic.Services.Orders.DTOs;
using SmartRestaurant.BusinessLogic.Services.Tables.DTOs;
using SmartRestaurant.Desktop.Components.Order;
using SmartRestaurant.Desktop.Service;
using SmartRestaurant.Desktop.Windows.BlurWindow;
using SmartRestaurant.Desktop.Windows.Extensions;
using SmartRestaurant.Domain.Const;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace SmartRestaurant.Desktop.Windows.Products;

/// <summary>
/// Interaction logic for OrderItemsViewWindow.xaml
/// </summary>
public partial class OrderItemsViewWindow : Window
{
    OrderDto _order;
    public event EventHandler<Guid>? OrderDeleted;
    public OrderItemsViewWindow(OrderDto order)
    {
        InitializeComponent();
        this._order = order;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);

        ShowOrderItems(_order.Items);
        waiter_name.Text = _order.OrderedUserName;
        if (_order.Status == OrderStatus.Canceled)
        {
            btn_canceled.Visibility = Visibility.Collapsed;
            user_name.Text = _order.CanceledUserName;
            time_at.Text = _order.CanceledAt?.ToString("yyyy-MM-dd HH:mm");
            time_reason.Text = "O'chirilgan vaqt: ";

        }
        else
        {
            st_deleting.Visibility = Visibility.Collapsed;
            time_reason.Text = "Yopilgan vaqt: ";
            time_at.Text = _order.UpdatedAt.ToString("yyyy-MM-dd HH:mm");
        }

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

    private void Close_Button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void ShowOrderItems(List<OrderItemDto> dto)
    {
        if (dto.Count > 0)
        {
            int count = 1;

            foreach (var item in dto)
            {
                OrderItemComponent itemComponent = new OrderItemComponent();
                itemComponent.SetData(item, count ++);
                st_OrderItems.Children.Add(itemComponent);
            }
        }
    }

    private Dictionary<string, List<OrderItemDto>> GroupOrdersByPrinter(List<OrderItemDto> items)
    {
        var printerGroups = new Dictionary<string, List<OrderItemDto>>();

        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.PrinterName) && item.PrinterName != "Printersiz")
            {
                if (!printerGroups.ContainsKey(item.PrinterName))
                {
                    printerGroups[item.PrinterName] = new List<OrderItemDto>();
                }
                printerGroups[item.PrinterName].Add(item);
            }
        }

        return printerGroups;
    }

    private void Resend_Button_Click(object sender, RoutedEventArgs e)
    {
        var kitchenProducts = GroupOrdersByPrinter(_order.Items);

        foreach (var printerGroup in kitchenProducts)
        {
            if (printerGroup.Value.Count > 0)
            {
                PrintService printService = new PrintService();
                printService.PrintChefChek(
                    new AddOrderDto { Items = printerGroup.Value, TableName = _order.TableName },
                    _order.QueueNumber,
                    printerGroup.Key
                );
            }
        }
    }

    private void Resend_ToKassa_Button_Click(object sender, RoutedEventArgs e)
    {
        PrintService printService = new PrintService();
        printService.PrintUserChek(_order, _order.TotalPrice, _order.TotalPrice);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBoxManager.ShowConfirmationWithOwner(this, "Siz haqiqatan ham bu buyurtmani bekor qilishni xohlaysizmi?"))
        {
            OrderDeleted?.Invoke(this, _order.Id);
            this.Close();
        }
    }
}
