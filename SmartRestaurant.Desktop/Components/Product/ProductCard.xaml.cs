using SmartRestaurant.BusinessLogic.Services.Products.DTOs;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SmartRestaurant.Desktop.Components;

/// <summary>
/// Interaction logic for ProductCard.xaml
/// </summary>
public partial class ProductCard : UserControl
{
    public event Action<ProductDto>? OnAddClicked;
    private ProductDto? _product;
    private NumberFormatInfo nfi;
    public ProductCard()
    {
        InitializeComponent();
        nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
    }

    public void SetData(ProductDto product)
    {
        product_name.Text = product.Name;
        product_price.Text = product.Price.ToString("#,0.##", nfi);
        _product = product;
    }

    public void SetProductImage(string imagePath)
    {
        try
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                productImage.Source = bitmap;
            }
        }
        catch (Exception ex)
        {
            // Handle exception (e.g., log it)
            Console.WriteLine($"Error loading image: {ex.Message}");
        }
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        OnAddClicked?.Invoke(_product!);
    }
}
