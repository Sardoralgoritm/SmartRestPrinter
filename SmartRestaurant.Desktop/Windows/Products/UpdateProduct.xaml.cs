using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SmartRestaurant.BusinessLogic.Services;
using SmartRestaurant.BusinessLogic.Services.Products.Concrete;
using SmartRestaurant.BusinessLogic.Services.Products.DTOs;
using SmartRestaurant.Desktop.Windows.BlurWindow;
using SmartRestaurant.Desktop.Windows.Extensions;
using SmartRestaurant.Domain.Entities;
using System.Drawing.Printing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static SmartRestaurant.Desktop.Windows.Extensions.NotificationWindow;

namespace SmartRestaurant.Desktop.Windows.Products;

/// <summary>
/// Interaction logic for UpdateProduct.xaml
/// </summary>
public partial class UpdateProduct : Window
{
    private readonly Guid _productId;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private string _originalImagePath = "";
    public event EventHandler? ProductUpdated;
    private string? _selectedImagePath;
    public UpdateProduct(Guid productId)
    {
        InitializeComponent();
        _productId = productId;
        _productService = App.ServiceProvider.GetRequiredService<IProductService>();
        _categoryService = App.ServiceProvider.GetRequiredService<ICategoryService>();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);
        await BindingFields();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!isValid())
                return;

            string imagePath = _originalImagePath;
            string oldImageFullPath = null!;
            string? newImageFullPath = null;

            if (!string.IsNullOrEmpty(_selectedImagePath))
            {
                string extension = Path.GetExtension(_selectedImagePath).ToLower();
                string imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductImages");

                if (!Directory.Exists(imagesDirectory))
                    Directory.CreateDirectory(imagesDirectory);

                string newFileName = $"{Guid.NewGuid()}{(extension == ".heic" ? ".png" : extension)}";
                newImageFullPath = Path.Combine(imagesDirectory, newFileName);
                imagePath = Path.Combine("ProductImages", newFileName); // DB uchun nisbiy

                try
                {
                    if (extension == ".heic")
                    {
                        await Task.Run(() =>
                        {
                            using (var image = new MagickImage(_selectedImagePath))
                            {
                                image.Format = MagickFormat.Png;
                                image.Write(newImageFullPath);
                            }
                        });
                    }
                    else
                    {
                        File.Copy(_selectedImagePath, newImageFullPath, overwrite: true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Yangi rasmni saqlashda xatolik: " + ex.Message);
                    return;
                }
            }


            var updatedProduct = new ProductDto
            {
                Id = _productId,
                Name = txtName.Text,
                Price = double.Parse(txtPrice.Text),
                CategoryId = (Guid)cmbCategory.SelectedValue,
                ImagePath = imagePath,
                PrinterName = (cmbPrinter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "",
            };

            var isSuccess = await _productService.UpdateAsync(updatedProduct);

            if (isSuccess)
            {
                if (oldImageFullPath != null && File.Exists(oldImageFullPath))
                {
                    try
                    {
                        File.Delete(oldImageFullPath);
                    }
                    catch (Exception ex)
                    {
                        // Xatolikni log qilish, lekin ishni to'xtatmaslik
                        Console.WriteLine($"Eski rasmni o'chirishda xatolik: {ex.Message}");
                    }
                }

                NotificationManager.ShowNotification(
                    MessageType.Success,
                    "Mahsulot muvaffaqiyatli yangilandi!",
                    NotificationPosition.TopRight);
                ProductUpdated?.Invoke(this, EventArgs.Empty);
                this.Close();
            }
            else
            {
                NotificationManager.ShowNotification(
                    MessageType.Error,
                    "Mahsulotni yangilash muvaffaqiyatsiz tugadi.",
                    NotificationPosition.TopRight,
                    4000);
            }
        }
        catch (Exception ex)
        {
            NotificationManager.ShowNotification(
                MessageType.Error,
                ex.Message,
                NotificationPosition.TopRight,
                5000);
        }
    }

    private bool isValid()
    {
        // Check if all required fields are filled
        if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPrice.Text) || cmbCategory.SelectedItem == null)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos barcha maydonlarni to'ldiring.");
            return false;
        }
        // Check if price is a valid decimal number
        if (!double.TryParse(txtPrice.Text, out _))
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos narxni to'g'ri kiriting.");
            return false;
        }

        // check the checkbox is selected
        if (cmbCategory.SelectedItem == null)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos toifani tanlang.");
            return false;
        }

        if (cmbPrinter.SelectedItem == null)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos printerni tanlang.");
            return false;
        }

        return true;
    }

    private async void product_Image_Border_MouseDown(object sender, MouseButtonEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Rasm fayllari|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.heic",
            Title = "Rasmni tanlang"
        };
        if (openFileDialog.ShowDialog() == true)
        {
            _selectedImagePath = openFileDialog.FileName;
            string extension = System.IO.Path.GetExtension(_selectedImagePath).ToLower();

            try
            {
                BitmapImage bitmap;

                ImageLoader.Visibility = Visibility.Visible;
                uploadPlaceholder.Visibility = Visibility.Collapsed;
                product_Image.Visibility = Visibility.Collapsed;


                if (extension == ".heic")
                {
                    bitmap = await Task.Run(() =>
                    {
                        using (var image = new MagickImage(_selectedImagePath))
                        {
                            image.Format = MagickFormat.Png;

                            using (var memStream = new MemoryStream())
                            {
                                image.Write(memStream);
                                memStream.Position = 0;

                                var bmp = new BitmapImage();
                                bmp.BeginInit();
                                bmp.CacheOption = BitmapCacheOption.OnLoad;
                                bmp.StreamSource = memStream;
                                bmp.EndInit();
                                bmp.Freeze(); // 🧊 UI threadga o‘tkazish uchun

                                return bmp;
                            }
                        }
                    });
                }
                else
                {
                    bitmap = new BitmapImage(new Uri(_selectedImagePath));
                }

                product_Image.Source = bitmap;
                ImageLoader.Visibility = Visibility.Collapsed;
                product_Image.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rasmni yuklashda xatolik: " + ex.Message);
            }
        }
    }

    private void product_Image_Border_MouseEnter(object sender, MouseEventArgs e)
    {
        Color veryLightBlue = Color.FromArgb(255, 230, 240, 250); // Very pale blue
        product_Image_Border.BorderBrush = new SolidColorBrush(veryLightBlue);

        // Show hover overlay effect
        hoverOverlay.Visibility = Visibility.Visible;

        // Make overlay content visible
        foreach (UIElement element in ((Panel)hoverOverlay.Child).Children)
        {
            element.Visibility = Visibility.Visible;
        }

        // Reduce opacity further (0.3 instead of 0.5)
        DoubleAnimation fadeIn = new DoubleAnimation(0, 0.3, TimeSpan.FromMilliseconds(200));
        hoverOverlay.BeginAnimation(Border.OpacityProperty, fadeIn);
    }

    private void product_Image_Border_MouseLeave(object sender, MouseEventArgs e)
    {
        product_Image_Border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));

        // Smooth fade-out animation
        DoubleAnimation fadeOut = new DoubleAnimation(0.3, 0, TimeSpan.FromMilliseconds(200));
        fadeOut.Completed += (s, _) =>
        {
            hoverOverlay.Visibility = Visibility.Collapsed;

            // Hide overlay content
            foreach (UIElement element in ((Panel)hoverOverlay.Child).Children)
            {
                element.Visibility = Visibility.Collapsed;
            }
        };

        hoverOverlay.BeginAnimation(Border.OpacityProperty, fadeOut);
    }

    private async Task BindingFields()
    {
        var prod = await _productService.GetByIdAsync(_productId);

        if (prod is null)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Mahsulotni yuklashda xatolik!");
            this.Close();
            return;
        }

        _originalImagePath = prod.ImagePath;
        txtName.Text = prod.Name;
        txtPrice.Text = prod.Price.ToString();
        var categories = await _categoryService.GetAllAsync();
        cmbCategory.ItemsSource = categories;
        cmbCategory.DisplayMemberPath = "Name";
        cmbCategory.SelectedValuePath = "Id";
        cmbCategory.SelectedValue = prod.CategoryId;
        SeedPrinters();
        SelectPrinterByName(prod.PrinterName);
        SetproductImage(prod.ImagePath);
    }

    private void SelectPrinterByName(string printerName)
    {
        if (string.IsNullOrEmpty(printerName) || printerName == "Printersiz")
        {
            cmbPrinter.SelectedIndex = 0;
            return;
        }

        foreach (ComboBoxItem item in cmbPrinter.Items)
        {
            if (item.Content.ToString() == printerName)
            {
                cmbPrinter.SelectedItem = item;
                return;
            }
        }

        cmbPrinter.SelectedIndex = 0;
    }

    private void SeedPrinters()
    {
        cmbPrinter.Items.Clear();

        ComboBoxItem noPrinterItem = new ComboBoxItem
        {
            Content = "Printersiz", // yoki "Chop etilmaydi"
            FontSize = 18
        };
        cmbPrinter.Items.Add(noPrinterItem);

        foreach (string printerName in PrinterSettings.InstalledPrinters)
        {
            ComboBoxItem item = new ComboBoxItem
            {
                Content = printerName,
                FontSize = 18
            };
            cmbPrinter.Items.Add(item);
        }
    }

    public void SetproductImage(string? imagePath)
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
                uploadPlaceholder.Visibility = Visibility.Collapsed;
                product_Image.Source = bitmap;
            }
        }
        catch (Exception ex)
        {
            // Handle exception (e.g., log it)
            Console.WriteLine($"Error loading image: {ex.Message}");
        }
    }

    private void Close_button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
