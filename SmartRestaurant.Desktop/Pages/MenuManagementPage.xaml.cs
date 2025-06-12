using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.BusinessLogic.Services;
using SmartRestaurant.BusinessLogic.Services.Products.Concrete;
using SmartRestaurant.Desktop.Components;
using SmartRestaurant.Desktop.Components.Loader;
using SmartRestaurant.Desktop.Windows.Categories;
using SmartRestaurant.Desktop.Windows.Extensions;
using SmartRestaurant.Desktop.Windows.Products;
using SmartRestaurant.Domain.Entities;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static SmartRestaurant.Desktop.Windows.Extensions.NotificationWindow;

namespace SmartRestaurant.Desktop.Pages;

/// <summary>
/// Interaction logic for MenuManagementPage.xaml
/// </summary>
public partial class MenuManagementPage : Page
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    private CancellationTokenSource _searchCts = null!;
    public MenuManagementPage()
    {
        InitializeComponent();
        _productService = App.ServiceProvider.GetRequiredService<IProductService>();
        _categoryService = App.ServiceProvider.GetRequiredService<ICategoryService>();
        this.TouchDown += (s, e) =>
        {
            MessageBox.Show("Touch keldi!");
        };
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)  
    {
        await LoadProducts();
        await LoadCategories();

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

    public async Task LoadProducts()
    {
        var option = new ProductSortFilterOptions();

        var products = await _productService.GetAllAsync(option);

        foreach (ProductComponent comp in st_Products.Children.OfType<ProductComponent>())
        {
            comp.ProductDeleted -= ProductComponent_ProductDeleted;
            comp.ProductUpdated -= ProductComponent_ProductUpdated;
            comp.ProductToggled -= ProductComponent_ProductToggled;
            comp.Dispose();
        }

        st_Products.Children.Clear();

        int count = 1;
        foreach (var product in products)
        {
            var productComponent = new ProductComponent(product.Id);
            productComponent.SeedData(product, count ++);
            productComponent.ProductDeleted += ProductComponent_ProductDeleted;
            productComponent.ProductUpdated += ProductComponent_ProductUpdated;
            productComponent.ProductToggled += ProductComponent_ProductToggled;

            st_Products.Children.Add(productComponent);
        }
    }

    private async void ProductComponent_ProductToggled(object? sender, Guid productId)
    {
        try
        {
            // Productni bazadan olib kelamiz
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                    "Mahsulot topilmadi!");
                return;
            }

            // Holatini teskari qilib saqlaymiz
            product.IsActive = !product.IsActive;

            var result = await _productService.UpdateAsync(product);

            if (!result)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                    "Mahsulotni yangilashda xatolik yuz berdi");
                return;
            }

            NotificationManager.ShowNotification(NotificationWindow.MessageType.Success,
                $"Mahsulot {(product.IsActive ? "faollashtirildi" : "noaktiv qilindi")}.");

            await LoadProducts();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Xatolik: {ex.Message}");
        }
    }

    private async void ProductComponent_ProductUpdated(object? sender, EventArgs e)
    {
        await LoadProducts();
        if (sender is UpdateProduct updateProduct)
        {
            updateProduct.ProductUpdated -= ProductComponent_ProductUpdated;
        }
    }

    public async Task LoadCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        foreach (CategoryComponent comp in st_Categories.Children.OfType<CategoryComponent>())
        {
            comp.CategoryDeleted -= CategoryComponent_CategoryDeleted;
            comp.Dispose();
        }

        st_Categories.Children.Clear();

        int count = 1;
        foreach (var category in categories)
        {
            var categoryComponent = new CategoryComponent(category.Id);
            categoryComponent.SeedData(category.Name, count ++);
            categoryComponent.CategoryDeleted += CategoryComponent_CategoryDeleted;
            categoryComponent.CategoryUpdated += CategoryComponent_CategoryUpdated;
            st_Categories.Children.Add(categoryComponent);
        }
    }

    private async void CategoryComponent_CategoryUpdated(object? sender, EventArgs e)
    {
        await LoadCategories();

        if (sender is UpdateCategory updateCategory)
        {
            updateCategory.CategoryUpdated -= CategoryComponent_CategoryUpdated;
        }
    }

    private async void CategoryComponent_CategoryDeleted(object? sender, Guid categoryId)
    {
        var category = await _categoryService.GetByIdAsync(categoryId);

        if (category == null)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                "Kategoriya topilmadi");
            return;
        }

        var isPossible = await _categoryService.IsDeletingPossibleAsync(categoryId);

        if (isPossible == false)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                "Ushbu kategoriya bog'langan mahsulotlar mavjud. O'chirish mumkin emas!");
            return;
        }

        var res = await _categoryService.DeleteAsync(categoryId);
        if (res)
        {
            NotificationManager.ShowNotification(
                MessageType.Success,
                $"Kategoriya muvaffaqiyatli o'chirildi.",
                NotificationPosition.TopCenter,
                3000);
            await LoadCategories();
        }
        else
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                "Kategoriya o'chirishda xatolik yuz berdi");
        }
    }

    public async void OnProductCreated(object? sender, EventArgs e)
    {
        await LoadProducts();

        if (sender is AddProduct addProduct)
        {
            addProduct.ProductAdded -= OnProductCreated;
        }
    }

    public async void OnCategoryCreated(object? sender, EventArgs e)
    {
        await LoadCategories();

        if (sender is AddCategory addCategory)
        {
            addCategory.CategoryAdded -= OnCategoryCreated;
        }
    }

    private async void ProductComponent_ProductDeleted(object? sender, Guid productId)
    {
        try
        {
            var product = await _productService.GetByIdAsync(productId);

            if (product == null)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                    "Mahsulot topilmadi");
                return;
            }

            string? imagePath = product.ImagePath;

            var res = await _productService.DeleteAsync(productId);
            if (res)
            {
                if (!string.IsNullOrEmpty(imagePath))
                {
                    try
                    {
                        string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        string fullImagePath = Path.Combine(appDirectory, imagePath);

                        if (File.Exists(fullImagePath))
                        {
                            File.Delete(fullImagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Rasmni o'chirishda xatolik: {ex.Message}");
                    }
                }

                NotificationManager.ShowNotification(
                    MessageType.Success,
                    $"Mahsulot muvaffaqiyatli o'chirildi.",
                    NotificationPosition.TopCenter,
                    3000);
                await LoadProducts();
            }
            else
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                    "Mahsulot o'chirishda xatolik yuz berdi");
            }
        }
        catch (Exception)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                "Mahsulot o'chirishda xatolik yuz berdi");
        }
    }

    private void Add_Product_Click(object sender, RoutedEventArgs e)
    {
        var addProduct = new AddProduct();
        addProduct.ProductAdded += OnProductCreated;
        addProduct.ShowDialog();
    }

    private void Add_Category_Click(object sender, RoutedEventArgs e)
    {
        this.TouchDown += (s, e) =>
        {
            MessageBox.Show("✅ Touch ishlayapti!");
        };
        var addCategory = new AddCategory();
        addCategory.CategoryAdded += OnCategoryCreated;
        addCategory.ShowDialog();
    }

    private async void tb_search_ProductTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        string keyword = tb_search_Product.Text.Trim();

        try
        {
            await Task.Delay(500, token);
        }
        catch { return; }

        if (token.IsCancellationRequested) return;

        if (string.IsNullOrWhiteSpace(keyword))
        {
            await LoadProducts();
            return;
        }

        st_Products.Children.Clear();
        Loader.Visibility = Visibility.Visible;
        EmptyData.Visibility = Visibility.Collapsed;

        var result = await _productService.GetAllAsync(new ProductSortFilterOptions
        {
            Name = keyword
        });

        Loader.Visibility = Visibility.Collapsed;

        if (result.Count > 0)
        {
            foreach (ProductComponent comp in st_Products.Children.OfType<ProductComponent>())
            {
                comp.ProductDeleted -= ProductComponent_ProductDeleted;
                comp.ProductUpdated -= ProductComponent_ProductUpdated;
                comp.ProductToggled -= ProductComponent_ProductToggled;
                comp.Dispose();
            }

            st_Products.Children.Clear();

            int count = 1;
            foreach (var product in result)
            {
                var productComponent = new ProductComponent(product.Id);
                productComponent.SeedData(product, count++);
                productComponent.ProductDeleted += ProductComponent_ProductDeleted;
                productComponent.ProductUpdated += ProductComponent_ProductUpdated;
                productComponent.ProductToggled += ProductComponent_ProductToggled;

                st_Products.Children.Add(productComponent);
            }
        }
        else
        {
            EmptyData.Visibility = Visibility.Visible;
        }
    }

    private void tb_search_Product_Loaded(object sender, RoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        var clearButton = textBox?.Template?.FindName("ClearButton", textBox) as Button;

        if (clearButton != null)
        {
            clearButton.Click += ClearButton_Click;
        }
    }

    private async void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        tb_search_Product.Clear();
        tb_search_Product.Focus();
        EmptyData.Visibility = Visibility.Collapsed;
        await LoadProducts();
    }
}
