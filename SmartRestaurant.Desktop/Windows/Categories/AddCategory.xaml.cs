using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.BusinessLogic.Services;
using SmartRestaurant.BusinessLogic.Services.Categories.DTOs;
using SmartRestaurant.Desktop.Windows.BlurWindow;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Windows;

namespace SmartRestaurant.Desktop.Windows.Categories;

/// <summary>
/// Interaction logic for AddCategory.xaml
/// </summary>
public partial class AddCategory : Window
{
    private readonly ICategoryService _categoryService;
    public event EventHandler? CategoryAdded;
    public AddCategory()
    {
        InitializeComponent();
        _categoryService = App.ServiceProvider.GetRequiredService<ICategoryService>();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(txtCategoryName.Text))
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos, kategoriya nomini kiriting.");
            return;
        }

        var newCategory = new AddCategoryDto
        {
            Name = txtCategoryName.Text,
        };

        var result = await _categoryService.CreateAsync(newCategory);

        if (result)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Success, "Kategoriya muvaffaqiyatli qo'shildi.");
            CategoryAdded?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
        else
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Kategoriya qo'shishda xatolik yuz berdi.");
        }
    }

    private void Close_button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();   
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);
        txtCategoryName.Focus();
    }
}
