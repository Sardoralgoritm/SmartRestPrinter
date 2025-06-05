using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SmartRestaurant.BusinessLogic.Services.Auth.Concrete;
using SmartRestaurant.BusinessLogic.Services.Auth.DTOs;
using SmartRestaurant.Desktop.Windows.BlurWindow;
using SmartRestaurant.Desktop.Windows.Extensions;
using SmartRestaurant.Domain.Const;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace SmartRestaurant.Desktop.Windows.Users;

/// <summary>
/// Interaction logic for AddUserWindow.xaml
/// </summary>
public partial class AddUserWindow : Window
{
    private readonly IAuthService _authService;
    public event EventHandler? UserAdded;
    private string? _selectedImagePath;
    public AddUserWindow()
    {
        InitializeComponent();
        _authService = App.ServiceProvider.GetRequiredService<IAuthService>();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!await isValid())
            return;

        string? imagePath = null;
        string? savedFullImagePath = null;

        try
        {
            // 1. Fayl saqlanadi (agar tanlangan bo‘lsa)
            if (!string.IsNullOrEmpty(_selectedImagePath))
            {
                string extension = Path.GetExtension(_selectedImagePath).ToLower();
                string fileName = $"{Guid.NewGuid()}{(extension == ".heic" ? ".png" : extension)}";
                string imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserImages");

                if (!Directory.Exists(imagesDirectory))
                    Directory.CreateDirectory(imagesDirectory);

                savedFullImagePath = Path.Combine(imagesDirectory, fileName);
                imagePath = $"UserImages/{fileName}";

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

            var newUser = new UserRegisterDto
            {
                FirstName = txtName.Text.Trim(),
                LastName = txtSurname.Text.Trim(),
                PhoneNumber = txtPhoneNumber.Text.Trim(),
                Role = (cmbRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? Roles.WAITER,
                Password = txtPassword.Text,
                ImageUrl = imagePath ?? ""
            };

            var result = await _authService.RegisterAsync(newUser);

            if (result)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Success, "Hodim muvaffaqiyatli qo'shildi.");
                UserAdded?.Invoke(this, EventArgs.Empty);
                this.Close();
            }
            else
            {
                // ❌ saqlangan faylni bekor qilish
                if (!string.IsNullOrEmpty(savedFullImagePath) && File.Exists(savedFullImagePath))
                {
                    File.Delete(savedFullImagePath);
                }

                NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Hodimni qo‘shib bo‘lmadi.");
            }
        }
        catch (DbUpdateException ex)
        {
            if (!string.IsNullOrEmpty(savedFullImagePath) && File.Exists(savedFullImagePath))
            {
                File.Delete(savedFullImagePath);
            }

            if (ex.InnerException?.Message.Contains("unique") == true)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Bu telefon raqam bilan foydalanuvchi mavjud.");
            }
            else
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Bazaga yozishda xatolik.");
            }

            _authService.ClearTracking();
        }
    }

    private async Task<bool> isValid()
    {

        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos hodim ismini kiriting.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtSurname.Text))
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos hodim familiyasini kiriting.");
            return false;
        }


        if (cmbRole.SelectedItem == null)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos lavozimni tanlang.");
            return false;
        }

        string phone = txtPhoneNumber.Text.Trim();
        if (!phone.StartsWith("+998") || phone.Length != 13)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning,
                "Telefon raqamni to‘g‘ri kiriting. Masalan: +998901234567");
            return false;
        }


        string password = txtPassword.Text;
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Parol kamida 6 ta belgidan iborat bo‘lishi kerak.");
            return false;
        }

        if (await _authService.IsPhoneNumberExistsAsync(txtPhoneNumber.Text.Trim()))
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Bu telefon raqam bilan foydalanuvchi allaqachon mavjud.");
            return false;
        }

        if (await _authService.IsPasswordExistAsync(txtPassword.Text.Trim()))
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Parolni o'zgartiring.");
            return false;
        }

        return true;
    }

    private void Close_button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);
    }

    private async void user_image_border_mouse_down(object sender, MouseButtonEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Rasm fayllari|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.heic",
            Title = "Rasmni tanlang"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            _selectedImagePath = openFileDialog.FileName;
            string extension = Path.GetExtension(_selectedImagePath).ToLower();

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

                user_image.Source = bitmap;
                ImageLoader.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rasmni yuklashda xatolik: " + ex.Message);
            }
        }
    }

    private void user_image_border_mouse_enter(object sender, MouseEventArgs e)
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

    private void user_image_border_mouse_leave(object sender, MouseEventArgs e)
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

    private void Window_Closed(object sender, EventArgs e)
    {
        user_image.Source = null;
    }
}
