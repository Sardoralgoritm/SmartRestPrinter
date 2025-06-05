using SmartRestaurant.BusinessLogic.Services.Orders.DTOs;
using SmartRestaurant.Desktop.Windows.Products;
using SmartRestaurant.Domain.Const;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Components;

/// <summary>
/// Interaction logic for OrderComponent.xaml
/// </summary>
public partial class OrderComponent : UserControl
{
    public event EventHandler<Guid>? OrderCanceled;
    private OrderDto? _order;
    public OrderComponent()
    {
        InitializeComponent();
    }

    public void SetData(OrderDto dto, int number)
    {
        _order = dto;
        tb_Number.Text = number.ToString();
        tb_transaction_id.Text = dto.TransactionId.ToString();
        tb_Table_Name.Text = dto.TableName;
        tb_waiter_name.Text = dto.ClosedUserName;
        tb_Order_Summa.Text = dto.TotalPrice.ToString(); 
    }

    private void View_Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        OrderItemsViewWindow window = new OrderItemsViewWindow(_order!);
        window.OrderDeleted += (s, e) =>
        {
            OrderCanceled?.Invoke(this, _order!.Id);
        };
        window.ShowDialog();
    }
}
