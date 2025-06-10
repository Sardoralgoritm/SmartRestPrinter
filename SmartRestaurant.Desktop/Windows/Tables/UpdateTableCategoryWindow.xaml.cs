using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.BusinessLogic.Services.Categories.DTOs;
using SmartRestaurant.BusinessLogic.Services.TableCategories.Concrete;
using SmartRestaurant.BusinessLogic.Services.TableCategories.DTOs;
using SmartRestaurant.Desktop.Windows.BlurWindow;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Windows;

namespace SmartRestaurant.Desktop.Windows.Tables
{
    /// <summary>
    /// Interaction logic for UpdateTableCategoryWindow.xaml
    /// </summary>
    public partial class UpdateTableCategoryWindow : Window
    {
        private readonly Guid _categoryId;
        private readonly ITableCategoryService _categoryService;
        public event EventHandler? CategoryUpdated;
        public UpdateTableCategoryWindow(Guid categoryId)
        {
            InitializeComponent();
            _categoryId = categoryId;
            _categoryService = App.ServiceProvider.GetRequiredService<ITableCategoryService>();
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

        private void Close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCategoryName.Text))
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos, kategoriya nomini kiriting.");
                return;
            }

            var updatedCategory = new TableCategoryDto
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
    }
}
