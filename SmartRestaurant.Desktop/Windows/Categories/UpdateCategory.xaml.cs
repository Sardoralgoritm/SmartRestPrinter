using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.BusinessLogic.Services;
using SmartRestaurant.BusinessLogic.Services.Categories.DTOs;
using SmartRestaurant.Desktop.Windows.BlurWindow;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Windows;

namespace SmartRestaurant.Desktop.Windows.Categories;

/// <summary>
/// Interaction logic for UpdateCategory.xaml
/// </summary>
public partial class UpdateCategory : Window
{
    private readonly Guid _categoryId;
    private readonly ICategoryService _categoryService;
    public event EventHandler? CategoryUpdated;
    public UpdateCategory(Guid categoryId)
    {
        InitializeComponent();
        _categoryService = App.ServiceProvider.GetRequiredService<ICategoryService>();
        _categoryId = categoryId;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);
        txtCategoryName.Focus();
        await BindingFields();
    }

    private async Task BindingFields()
    {
        var category = await _categoryService.GetByIdAsync(_categoryId);

        if (category is null)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Kategoriyani yuklashda xatolik!");
            this.Close();
            return;
        }

        txtCategoryName.Text = category.Name;
    }

    private async void Update_Click(object sender, RoutedEventArgs e)
    {
        var updatedCategory = new CategoryDto
        {
            Id = _categoryId,
            Name = txtCategoryName.Text
        };

        var result = await _categoryService.UpdateAsync(updatedCategory);
        if (result)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Success,
                "Kategoriya muvaffaqiyatli yangilandi!");
            CategoryUpdated?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
        else
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, 
                "Kategoriyani yangilash muvaffaqqiyatsiz tugadi!");
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void Close_button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
