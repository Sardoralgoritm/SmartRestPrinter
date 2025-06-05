using System.Windows.Controls;
using SmartRestaurant.BusinessLogic.Services.Tables.Concrete;
using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.Desktop.Components;
using SmartRestaurant.Desktop.Windows.Tables;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Windows.Media;
using System.Windows;

namespace SmartRestaurant.Desktop.Pages;

/// <summary>
/// Interaction logic for TablePage.xaml
/// </summary>
public partial class TablePage : Page
{
    private readonly ITableService _tableService;

    public TablePage()
    {
        InitializeComponent();
        _tableService = App.ServiceProvider.GetRequiredService<ITableService>();
    }


    private void AddTable_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var addTableWindow = new AddTableWindow();
        addTableWindow.TableAdded += OnTableCreated;
        addTableWindow.ShowDialog();
    }

    private async void OnTableCreated(object? sender, EventArgs e)
    {
        await LoadTables();

        if (sender is AddTableWindow addTableWindow)
        {
            addTableWindow.TableAdded -= OnTableCreated;
        }
    }

    private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        await LoadTables();

        foreach (var scrollViewer in FindVisualChildren<ScrollViewer>(this))
        {
            scrollViewer.ManipulationBoundaryFeedback += (s, args) =>
            {
                args.Handled = true;
            };
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
            {
                yield return t;
            }
            foreach (var childOfChild in FindVisualChildren<T>(child))
            {
                yield return childOfChild;
            }
        }
    }

    public async Task LoadTables()
    {
        var tables = await _tableService.GetAllAsync(new BusinessLogic.Services.Tables.TableSortFilterOption());

        foreach (TableCRUDComponent comp in spTables.Children.OfType<TableCRUDComponent>())
        {
            comp.TableDeleted -= TableCRUDComponent_TableDeleted;
            comp.Dispose();
        }

        spTables.Children.Clear();

        foreach (var table in tables)
        {
            var tableCRUDComponent = new TableCRUDComponent(table.Id);
            tableCRUDComponent.SetTableData(table.Name, table.Status);
            tableCRUDComponent.TableDeleted += TableCRUDComponent_TableDeleted;
            tableCRUDComponent.TableUpdated += TableCRUDComponent_TableUpdated;
            spTables.Children.Add(tableCRUDComponent);
        }
    }

    private async void TableCRUDComponent_TableUpdated(object? sender, EventArgs e)
    {
        await LoadTables();
        if (sender is TableCRUDComponent tableCRUDComponent)
        {
            tableCRUDComponent.TableUpdated -= TableCRUDComponent_TableUpdated;
        }
    }

    private async void TableCRUDComponent_TableDeleted(object? sender, Guid tableId)
    {
        var result = await _tableService.DeleteAsync(tableId);

        if (result)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Success, "Stol muvaffaqiyatli o'chirildi.");
            await LoadTables();
        }
        else
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Stol o'chirishda xatolik yuz berdi.");
        }
    }
}
