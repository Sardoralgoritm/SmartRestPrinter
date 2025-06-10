using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.BusinessLogic.Services.TableCategories.Concrete;
using SmartRestaurant.BusinessLogic.Services.TableCategories.DTOs;
using SmartRestaurant.Desktop.Windows.BlurWindow;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Windows;

namespace SmartRestaurant.Desktop.Windows.Tables
{
    /// <summary>
    /// Interaction logic for AddTableWindowCategory.xaml
    /// </summary>
    public partial class AddTableCategoryWindow : Window
    {
        private readonly ITableCategoryService _categoryService;
        public event EventHandler? CategoryAdded;
        public AddTableCategoryWindow()
        {
            InitializeComponent();
            _categoryService = App.ServiceProvider.GetRequiredService<ITableCategoryService>();
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
            if (string.IsNullOrEmpty(txtCategoryName.Text))
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos, kategoriya nomini kiriting.");
                return;
            }

            var newCategory = new AddTableCategoryDto
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BlurEffect.EnableBlur(this);
            txtCategoryName.Focus();
        }
    }
}
