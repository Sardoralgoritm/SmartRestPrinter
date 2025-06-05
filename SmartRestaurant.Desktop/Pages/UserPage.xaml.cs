using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.BusinessLogic.Services.Users.Concrete;
using SmartRestaurant.Desktop.Components.User;
using SmartRestaurant.Desktop.Helpers.ScrollCustomization;
using SmartRestaurant.Desktop.Windows.Extensions;
using SmartRestaurant.Desktop.Windows.Users;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Pages;

/// <summary>
/// Interaction logic for UserPage.xaml
/// </summary>
public partial class UserPage : Page
{
    private readonly IUserService _userService;
    public UserPage()
    {
        InitializeComponent();
        _userService = App.ServiceProvider.GetRequiredService<IUserService>();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadUsers();
        //this.DisableTouchScrollBounce();
    }

    private async Task LoadUsers()
    {
        var users = await _userService.GetAllUsersAsync();

        // Eski komponentlarni tozalaymiz
        foreach (UserComponent comp in spUsers.Children.OfType<UserComponent>())
        {
            comp.Dispose();
        }

        spUsers.Children.Clear();

        if (users == null || users.Count == 0)
        {
            EmptyData.Visibility = Visibility.Visible;
            return;
        }

        EmptyData.Visibility = Visibility.Collapsed;

        int index = 1;
        foreach (var user in users)
        {
            var userComponent = new UserComponent();
            userComponent.SetUserData(user, index ++);

            userComponent.UserDeleted += UserComponent_UserDeleted;
            userComponent.UserUpdated += UserComponent_UserUpdated;
            userComponent.UserViewed += UserComponent_UserViewed;

            spUsers.Children.Add(userComponent);
        }
    }

    private void AddUser_Click(object sender, RoutedEventArgs e)
    {
        var addUserWindow = new AddUserWindow();
        addUserWindow.UserAdded += OnUserCreated;
        addUserWindow.ShowDialog();
    }

    private async void OnUserCreated(object? sender, EventArgs e)
    {
        await LoadUsers();

        if (sender is AddUserWindow addUserWindow)
        {
            addUserWindow.UserAdded -= OnUserCreated;
        }
    }

    private async void UserComponent_UserDeleted(object? sender, Guid userId)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                    "Foydalanuvchi topilmadi");
                return;
            }

            string? imagePath = user.ImageUrl;

            var result = await _userService.DeleteUserAsync(userId);
            if (result)
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
                        Console.WriteLine($"Rasmni o‘chirishda xatolik: {ex.Message}");
                    }
                }

                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Success,
                    "Foydalanuvchi muvaffaqiyatli o‘chirildi.");
                await LoadUsers();
            }
            else
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                    "Foydalanuvchini o‘chirishda xatolik yuz berdi.");
            }
        }
        catch (Exception ex)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                "Foydalanuvchini o‘chirishda xatolik yuz berdi.");
            Console.WriteLine(ex);
        }
    }

    private void UserComponent_UserUpdated(object? sender, Guid userId)
    {
        var editWindow = new EditUserWindow(userId);
        editWindow.UserUpdated += async (s, e) =>
        {
            await LoadUsers();
        };
        editWindow.ShowDialog();

        //if (sender is UserComponent userComponent)
        //{
        //    userComponent.UserUpdated -= UserComponent_UserUpdated;
        //}
    }

    private async void UserComponent_UserViewed(object? sender, Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);

        if (user is not null)
        {
            var window = new ViewUserWindow(user);
            window.ShowDialog();
        }
    }
}
