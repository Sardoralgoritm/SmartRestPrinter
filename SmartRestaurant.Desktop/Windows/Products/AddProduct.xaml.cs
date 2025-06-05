using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SmartRestaurant.BusinessLogic.Services;
using SmartRestaurant.BusinessLogic.Services.Products.Concrete;
using SmartRestaurant.BusinessLogic.Services.Products.DTOs;
using SmartRestaurant.Desktop.Windows.BlurWindow;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Drawing.Printing;
using System.IO;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace SmartRestaurant.Desktop.Windows.Products;

/// <summary>
/// Interaction logic for AddProduct.xaml
/// </summary>
public partial class AddProduct : Window
{
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;
    public event EventHandler? ProductAdded;
    private string? _selectedImagePath;
    public AddProduct()
    {
        InitializeComponent();
        _categoryService = App.ServiceProvider.GetRequiredService<ICategoryService>();
        _productService = App.ServiceProvider.GetRequiredService<IProductService>();
    }
    private void Close_button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!isValid())
            return;

        string? imagePath = null;
        string? savedFullImagePath = null;

        if (!string.IsNullOrEmpty(_selectedImagePath))
        {
            string extension = Path.GetExtension(_selectedImagePath).ToLower();
            string fileName = $"{Guid.NewGuid()}{(extension == ".heic" ? ".png" : extension)}";
            string imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductImages");

            if (!Directory.Exists(imagesDirectory))
                Directory.CreateDirectory(imagesDirectory);

            savedFullImagePath = Path.Combine(imagesDirectory, fileName);
            imagePath = $"ProductImages/{fileName}";

            if (extension == ".heic")
            {
                await Task.Run(() =>
                {
                    using (var image = new MagickImage(_selectedImagePath))
                    {
                        image.Format = MagickFormat.Png;
                        image.Write(savedFullImagePath);
                    }
                });
            }
            else
            {
                File.Copy(_selectedImagePath, savedFullImagePath, overwrite: true);
            }
        }

        var newProduct = new AddProductDto
        {
            Name = txtName.Text,
            Price = double.Parse(txtPrice.Text),
            CategoryId = (Guid)cmbCategory.SelectedValue,
            ImagePath = imagePath ?? "",
            PrinterName = (cmbPrinter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? ""
        };

        var isCreated = await _productService.AddAsync(newProduct);

        if (isCreated)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Success, "Mahsulot muvaffaqiyatli qo'shildi.");
            ProductAdded?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
        else
        {
            if (!string.IsNullOrEmpty(savedFullImagePath) && File.Exists(savedFullImagePath))
            {
                File.Delete(savedFullImagePath);
            }
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Mahsulot qo'shishda xatolik yuz berdi.");
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

        if (cmbPrinter.SelectedItem == null) {
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
                                bmp.Freeze();

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

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);
        var categories = await _categoryService.GetAllAsync();

        cmbCategory.ItemsSource = categories;
        cmbCategory.DisplayMemberPath = "Name";
        cmbCategory.SelectedValuePath = "Id";
        SeedPrinters();
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
}
