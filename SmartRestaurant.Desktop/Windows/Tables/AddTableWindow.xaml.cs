using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.BusinessLogic.Services.TableCategories.Concrete;
using SmartRestaurant.BusinessLogic.Services.Tables.Concrete;
using SmartRestaurant.BusinessLogic.Services.Tables.DTOs;
using SmartRestaurant.Desktop.Windows.BlurWindow;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Windows;

namespace SmartRestaurant.Desktop.Windows.Tables;

/// <summary>
/// Interaction logic for AddTableWindow.xaml
/// </summary>
public partial class AddTableWindow : Window
{
    private readonly ITableService _tableService;
    private readonly ITableCategoryService _tableCategoryService;
    public event EventHandler? TableAdded;
    public AddTableWindow()
    {
        InitializeComponent();
        _tableService = App.ServiceProvider.GetRequiredService<ITableService>();
        _tableCategoryService = App.ServiceProvider.GetRequiredService<ITableCategoryService>();
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
            if (string.IsNullOrWhiteSpace(txtTableName.Text))
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos, stol nomini kiriting.");
                return;
            }

        if (cmbCategory.SelectedItem == null)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Iltimos kategoriyani tanlang.");
            return;
        }

        string[] parts = txtTableName.Text.Trim().Split(' ');

            // Kamida 1 ta qism bo‘lishi kerak
            string numberPart = parts.Length == 1 ? parts[0] : parts[^1];

            if (parts.Length < 2)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Stol nomi kamida 2 ta qismdan iborat bo'lishi kerak. Masalan: 'Table 1' yoki 'Stol 1'.");
                return;
            }

            if (!int.TryParse(numberPart, out _))
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning, "Stol nomining oxiri raqam bo‘lishi kerak. Masalan: '1' yoki 'Table 1'.");
                return;
            }

            var newTable = new AddTableDto
            {
                Name = txtTableName.Text,
                TableCategoryId = (Guid?)cmbCategory.SelectedValue
            };

        var result = await _tableService.CreateAsync(newTable);
        if (result)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Success, "Stol muvaffaqiyatli qo'shildi.");
            TableAdded?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
        else
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Stol qo'shishda xatolik yuz berdi.");
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BlurEffect.EnableBlur(this);
        txtTableName.Focus();
        SeedCatgories();
    }

    private async void SeedCatgories()
    {
        var categories = await _tableCategoryService.GetAllAsync();
        cmbCategory.ItemsSource = categories;
        cmbCategory.DisplayMemberPath = "Name";
        cmbCategory.SelectedValuePath = "Id";
    }
}
