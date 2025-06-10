using SmartRestaurant.Desktop.Windows.Categories;
using SmartRestaurant.Desktop.Windows.Extensions;
using SmartRestaurant.Desktop.Windows.Tables;
using System.Windows;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Components.TableCategory
{
    /// <summary>
    /// Interaction logic for TableCategoryComponent.xaml
    /// </summary>
    public partial class TableCategoryComponent : UserControl, IDisposable
    {
        public event EventHandler<Guid>? CategoryDeleted;
        public event EventHandler? CategoryUpdated;
        private Guid _categoryId;
        public TableCategoryComponent(Guid categoryId)
        {
            InitializeComponent();
            _categoryId = categoryId;
        }

        public void SeedData(string categoryName, int count)
        {
            no.Text = count.ToString();
            c_name.Text = categoryName;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var editCategoryWindow = new UpdateTableCategoryWindow(_categoryId);
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

        public void Dispose()
        {
            CategoryDeleted = null;
        }
    }
}
