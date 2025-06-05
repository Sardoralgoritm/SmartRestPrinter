using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SmartRestaurant.BusinessLogic.Services.Auth.Concrete;
using SmartRestaurant.BusinessLogic.Services.Users.Concrete;
using SmartRestaurant.BusinessLogic.Services.Users.DTOs;
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
using static SmartRestaurant.Desktop.Windows.Extensions.NotificationWindow;

namespace SmartRestaurant.Desktop.Windows.Users;

/// <summary>
/// Interaction logic for EditUserWindow.xaml
/// </summary>
public partial class EditUserWindow : Window
{
    private Guid _userId;
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private string _originalImagePath = "";
    public event EventHandler? UserUpdated;
    private string? _selectedImagePath;
    public EditUserWindow(Guid userId)
    {
        InitializeComponent();
        _userId = userId;
        _userService = App.ServiceProvider.GetRequiredService<IUserService>();
        _authService = App.ServiceProvider.GetRequiredService<IAuthService>();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void Close_button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!await isValid())
            {
                return;
            }

            string imagePath = _originalImagePath;
            string oldImageFullPath = null!;
            string? newImageFullPath = null;

            if (!string.IsNullOrEmpty(_selectedImagePath))
            {
                string extension = Path.GetExtension(_selectedImagePath).ToLower();
                string imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserImages");

                if (!Directory.Exists(imagesDirectory))
                    Directory.CreateDirectory(imagesDirectory);

                string newFileName = $"{Guid.NewGuid()}{(extension == ".heic" ? ".png" : extension)}";
                newImageFullPath = Path.Combine(imagesDirectory, newFileName);
                imagePath = Path.Combine("UserImages", newFileName); // DB uchun nisbiy

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

            // USER DTO
            var updatedUser = new UserDto
            {
                Id = _userId,
                FirstName = txtName.Text,
                LastName = txtSurname.Text,
                PhoneNumber = txtPhoneNumber.Text,
                Password = txtPassword.Text,
                ImageUrl = imagePath,
                Role = (cmbRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? Roles.WAITER
            };

            // YANGILASH
            var isSuccess = await _userService.UpdateUserAsync(updatedUser);

            if (isSuccess)
            {
                // Eski rasmni o‘chirish
                if (!string.IsNullOrEmpty(_originalImagePath))
                {
                    oldImageFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _originalImagePath);
                    if (File.Exists(oldImageFullPath))
                    {
                        try { File.Delete(oldImageFullPath); } catch { }
                    }
                }

                NotificationManager.ShowNotification(
                    MessageType.Success,
                    "Hodim muvaffaqiyatli yangilandi!",
                    NotificationPosition.TopRight);

                UserUpdated?.Invoke(this, EventArgs.Empty);
                this.Close();
            }
            else
            {
                // Yangi saqlangan rasmni bekor qilish
                if (!string.IsNullOrEmpty(newImageFullPath) && File.Exists(newImageFullPath))
                {
                    try { File.Delete(newImageFullPath); } catch { }
                }

                NotificationManager.ShowNotification(
                    MessageType.Error,
                    "Hodimni yangilash muvaffaqiyatsiz tugadi.",
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
                user_image.Visibility = Visibility.Collapsed;

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
                user_image.Visibility = Visibility.Visible;
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

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);
        await BindingFields();
    }

    private async Task BindingFields()
    {
        var user = await _userService.GetUserByIdAsync(_userId);

        if (user is null)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Hodimni yuklashda xatolik!");
            this.Close();
            return;
        }

        _originalImagePath = user.ImageUrl;
        txtName.Text = user.FirstName;
        txtSurname.Text = user.LastName;
        txtPhoneNumber.Text = user.PhoneNumber;
        SetUserImage(user.ImageUrl);

        var roles = new List<string> { "Boss", "Ofitsiant", "Manager" };
        cmbRole.Items.Clear();

        foreach (var role in roles)
        {
            var item = new ComboBoxItem { Content = role };
            cmbRole.Items.Add(item);

            if (role == user.Role)
            {
                cmbRole.SelectedItem = item;
            }
        }
    }

    public void SetUserImage(string? imagePath)
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
                user_image.Source = bitmap;
            }
        }
        catch (Exception ex)
        {
            // Handle exception (e.g., log it)
            Console.WriteLine($"Error loading image: {ex.Message}");
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

        string phone = txtPhoneNumber.Text.Trim();
        if (!phone.StartsWith("+998") || phone.Length != 13)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning,
                "Telefon raqamni to‘g‘ri kiriting. Masalan: +998901234567");
            return false;
        }

        var currentUser = await _userService.GetUserByIdAsync(_userId);

        if (currentUser == null) {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Hodimni topishda xatolik!");
            return false;
        }

        if (currentUser.PhoneNumber != txtPhoneNumber.Text.Trim())
        {
            if (await _authService.IsPhoneNumberExistsAsync(txtPhoneNumber.Text.Trim()))
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Bu telefon raqam bilan foydalanuvchi allaqachon mavjud.");
                return false;
            }
        }

        if (txtPassword.Text.Length > 0 && await _authService.IsPasswordExistAsync(txtPassword.Text.Trim()))
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Parolni o'zgartiring.");
            return false;
        }

        return true;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        user_image.Source = null;
    }
}
