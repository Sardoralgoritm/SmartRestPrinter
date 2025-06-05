using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.BusinessLogic.Services.Auth.Concrete;
using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;
using SmartRestaurant.Desktop.Windows.Orders;
using System.Windows;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Components;

/// <summary>
/// Interaction logic for OrderSummaryItem.xaml
/// </summary>
public partial class OrderSummaryItem : UserControl
{
    public Guid ProductId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public double Quantity { get; private set; }
    public event EventHandler<Guid>? QuantityDecreased;
    public event EventHandler<string>? BossPasswordEntered;

    public OrderSummaryItem()
    {
        InitializeComponent();
    }

    public void SeedData(OrderItemDto dto)
    {
        ProductId = dto.ProductId;
        txtProductName.Text = dto.ProductName;
        txtProductPrice.Text = dto.ProductPrice.ToString();
        Quantity = dto.Quantity;
        OrderItemId = dto.Id;
        txtProductCount.Text = Quantity.ToString();
    }

    public void UpdateQuantity(double quantity)
    {
        Quantity = quantity;
        txtProductCount.Text = Quantity.ToString();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        if (Quantity > 1)
        {
            Quantity--;
            txtProductCount.Text = Quantity.ToString();
        }

        QuantityDecreased?.Invoke(this, ProductId);
    }

    private void Boss_Cancel_Click(object sender, RoutedEventArgs e)
    {
        var cancelWindow = new CancelOrderWindow();
        var result = cancelWindow.ShowDialog();

        if (result == true)
        {
            // Foydalanuvchi parolni kiritdi
            BossPasswordEntered?.Invoke(this, cancelWindow.Password);
        }
    }
}
