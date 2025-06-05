using SmartRestaurant.Desktop.Windows.Extensions;
using SmartRestaurant.Desktop.Windows.Tables;
using System.Windows;
using System.Windows.Controls;

namespace SmartRestaurant.Desktop.Components
{
    /// <summary>
    /// Interaction logic for TableCRUDComponent.xaml
    /// </summary>
    public partial class TableCRUDComponent : UserControl, IDisposable
    {
        public event EventHandler<Guid>? TableDeleted;
        public event EventHandler? TableUpdated;

        private Guid _tableId;
        public void Dispose()
        {
            TableDeleted = null;
        }


        public TableCRUDComponent(Guid tableId)
        {
            InitializeComponent();
            _tableId = tableId;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var editTableWindow = new UpdateTableWindow(_tableId);
            editTableWindow.TableUpdated += (s, e) =>
            {
                TableUpdated?.Invoke(this, EventArgs.Empty);
            };
            editTableWindow.ShowDialog();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxManager.ShowConfirmation("Siz haqiqatan ham bu stulni o'chirishni xohlaysizmi?"))
            {
                TableDeleted?.Invoke(this, _tableId);
            }   
        }

        public void SetTableData(string tableName, string tableStatus)
        {
            txtNumber.Text = tableName;
            txtStatus.Text = tableStatus;
        }
    }
}
