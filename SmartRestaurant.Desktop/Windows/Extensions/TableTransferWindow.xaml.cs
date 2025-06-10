using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.BusinessLogic.Services.TableCategories.Concrete;
using SmartRestaurant.BusinessLogic.Services.Tables;
using SmartRestaurant.BusinessLogic.Services.Tables.Concrete;
using SmartRestaurant.BusinessLogic.Services.Tables.DTOs;
using SmartRestaurant.Domain.Const;
using System.Windows;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Windows.Extensions;

public partial class TableTransferWindow : Window
{
    private readonly ITableCategoryService _tableCategoryService;
    private readonly ITableService _tableService;
    private readonly string _tableName;
    private Button selectedTableButton = null;
    private Guid selectedTableId;
    public EventHandler<Guid> tableIdChanged;
    public TableTransferWindow(TableDto table)
    {
        InitializeComponent();
        _tableCategoryService = App.ServiceProvider.GetRequiredService<ITableCategoryService>();
        _tableService = App.ServiceProvider.GetRequiredService<ITableService>();
        _tableName = table.Name;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void TransferButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (selectedTableId == null || selectedTableButton == null)
            {
                NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning,
                    "Iltimos, stolni tanlang!",
                    NotificationWindow.NotificationPosition.TopCenter);
                return;
            }

            string tableName = selectedTableButton.Content.ToString();

            var result = MessageBoxManager.ShowConfirmation(
                $"Buyurtmani {tableName} ga ko'chirasizmi?");

            if (result)
            {
                tableIdChanged.Invoke(this, selectedTableId);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ko'chirishda xatolik: {ex.Message}");
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        HeaderSubtitle.Text = $"{_tableName} dan boshqa stolga ko'chirish";
        await LoadTableCategoriesAsync();
    }

    private async Task LoadTableCategoriesAsync()
    {
        try
        {
            var categories = await _tableCategoryService.GetAllAsync();

            spTableCategoryButtons.Children.Clear();

            foreach (var category in categories)
            {
                var rb = new RadioButton
                {
                    Content = category.Name,
                    Tag = category.Id,
                    Style = (Style)FindResource("RadioCategoryStyle")
                };

                rb.Checked += async (s, e) =>
                {
                    var categoryId = (Guid)((RadioButton)s).Tag;
                    await LoadTablesAsync(categoryId);
                };

                spTableCategoryButtons.Children.Add(rb);
            }

            if (spTableCategoryButtons.Children.Count > 0 &&
                spTableCategoryButtons.Children[0] is RadioButton firstRb)
            {
                firstRb.IsChecked = true;
            }
        }
        catch (Exception ex)
        {
            MessageBoxManager.ShowError("Kategoriyalarni yuklashda xatolik");
        }
    }

    private async Task LoadTablesAsync(Guid categoryId)
    {
        try
        {
            var option = new TableSortFilterOption
            {
                CategoryId = categoryId,
                Status = TableStatus.Free
            };

            var tables = await _tableService.GetAllAsync(option);

            DynamicTablesGrid.Children.Clear();

            selectedTableButton = null;

            foreach (var table in tables)
            {
                var tableButton = new Button
                {
                    Content = table.Name,
                    Tag = table.Id,
                    Style = (Style)FindResource("TableButton")
                };

                tableButton.Click += TableButton_Click;

                DynamicTablesGrid.Children.Add(tableButton);
            }

            int columns = Math.Min(3, Math.Max(1, tables.Count()));
            DynamicTablesGrid.Columns = columns;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Stollarni yuklashda xatolik: {ex.Message}");
        }
    }

    private void TableButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Button clickedTable = sender as Button;
            if (clickedTable != null)
            {
                if (selectedTableButton != null)
                {
                    selectedTableButton.Style = (Style)FindResource("TableButton");
                }

                selectedTableButton = clickedTable;
                selectedTableId = (Guid)clickedTable.Tag;

                clickedTable.Style = (Style)FindResource("SelectedTableButton");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Stol tanlashda xatolik: {ex.Message}");
        }
    }
}
