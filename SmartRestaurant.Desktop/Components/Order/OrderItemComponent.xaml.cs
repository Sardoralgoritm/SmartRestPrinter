using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Components.Order;

/// <summary>
/// Interaction logic for OrderItemComponent.xaml
/// </summary>
public partial class OrderItemComponent : UserControl
{
    public OrderItemComponent()
    {
        InitializeComponent();
    }

    public void SetData(OrderItemDto dto, int count)
    {
        tb_Number.Text = count.ToString();
        tb_Product_Name.Text = dto.ProductName;
        tb_Product_Category.Text = dto.CategoryName;
        tb_Product_Price.Text = dto.TotalPrice.ToString();
        tb_Product_Quantity.Text = dto.Quantity.ToString();
    }
}
