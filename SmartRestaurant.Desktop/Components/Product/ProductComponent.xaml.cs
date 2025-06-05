using SmartRestaurant.BusinessLogic.Services.Products.DTOs;
using SmartRestaurant.Desktop.Windows.Extensions;
using SmartRestaurant.Desktop.Windows.Products;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Components
{
    /// <summary>
    /// Interaction logic for ProductComponent.xaml
    /// </summary>
    public partial class ProductComponent : UserControl, IDisposable
    {
        public event EventHandler<Guid>? ProductDeleted;
        public event EventHandler? ProductUpdated;
        public event EventHandler<Guid>? ProductToggled;

        private NumberFormatInfo nfi;
        private Guid _productId;
        public ProductComponent(Guid productId)
        {
            InitializeComponent();
            _productId = productId;
            nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxManager.ShowConfirmation("Siz haqiqatan ham bu maxsulotni o'chirishni xohlaysizmi?"))
            {
                ProductDeleted?.Invoke(this, _productId);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var udpateProductWindow = new UpdateProduct(_productId);
            udpateProductWindow.ProductUpdated += (s, e) =>
            {
                ProductUpdated?.Invoke(this, EventArgs.Empty);
            };
            udpateProductWindow.ShowDialog();
        }

        public void SeedData(ProductDto product, int number)
        {
            no.Text = number.ToString();
            p_name.Text = product.Name;
            p_price.Text = product.Price.ToString("#,0.##", nfi);
            p_category.Text = product.CategoryName;
            IsActiveToggle.IsChecked = product.IsActive;
        }

        public void Dispose()
        {
            ProductDeleted = null;
            ProductUpdated = null;
            ProductToggled = null;
        }

        private void IsActiveToggle_Checked(object sender, RoutedEventArgs e)
        {
            ProductToggled?.Invoke(this, _productId);
        }

        private void IsActiveToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ProductToggled?.Invoke(this, _productId);
        }
    }
}
