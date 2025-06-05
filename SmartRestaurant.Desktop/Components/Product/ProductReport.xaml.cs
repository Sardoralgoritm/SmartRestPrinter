using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;
using SmartRestaurant.Domain.Entities;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Components.Product
{
    /// <summary>
    /// Interaction logic for ProductReport.xaml
    /// </summary>
    public partial class ProductReport : UserControl
    {
        public ProductReport()
        {
            InitializeComponent();
        }

        public void SeedData(OrderItemDto dto, int number)
        {
            tb_Number.Text = number.ToString();
            tb_product_name.Text = dto.ProductName;
            tb_product_count.Text = dto.Quantity.ToString();
            tb_total_sum.Text = dto.TotalPrice.ToString();
        }
    }
}
