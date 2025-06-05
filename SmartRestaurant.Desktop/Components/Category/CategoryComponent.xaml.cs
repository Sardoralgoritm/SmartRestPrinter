using SmartRestaurant.Desktop.Windows.Categories;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Components;

/// <summary>
/// Interaction logic for CategoryComponent.xaml
/// </summary>
public partial class CategoryComponent : UserControl, IDisposable
{
    public event EventHandler<Guid>? CategoryDeleted;
    public event EventHandler? CategoryUpdated;

    private Guid _categoryId;
    public CategoryComponent(Guid categoryId)
    {
        InitializeComponent();
        _categoryId = categoryId;
    }

    public void Dispose()
    {
        CategoryDeleted = null;
    }
    public void SeedData(string categoryName, int count)
    {
        no.Text = count.ToString();
        c_name.Text = categoryName;
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        var editCategoryWindow = new UpdateCategory(_categoryId);
        editCategoryWindow.CategoryUpdated += (s, e) =>
        {
            CategoryUpdated?.Invoke(this, EventArgs.Empty);
        };

        editCategoryWindow.ShowDialog();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBoxManager.ShowConfirmation("Siz haqiqatan ham bu kategoriyani o'chirishni xohlaysizmi?"))
        {
            CategoryDeleted?.Invoke(this, _categoryId);
        }
    }
}
