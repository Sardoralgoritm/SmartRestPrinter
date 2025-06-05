using SmartRestaurant.BusinessLogic.Services.Tables.DTOs;
using SmartRestaurant.Domain.Const;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace SmartRestaurant.Desktop.Components;

/// <summary>
/// Interaction logic for TableComponent.xaml
/// </summary>
public partial class TableComponent : UserControl
{
    public bool IsSelected { get; private set; } = false;

    public TableComponent()
    {
        InitializeComponent();
    }

    public void SeedData(TableDto dto)
    {
        DataContext = dto;

        txtTableName.Text = dto.Name;
        txtStatus.Text = dto.Status;

        ApplyStatusStyle(dto.Status);
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (selected)
        {
            border.Background = (Brush)FindResource("SelectedTableBackground");
            border.BorderBrush = (Brush)FindResource("SelectedTableBorderBrush");
            border.BorderThickness = new Thickness(2);
            border.Effect = (Effect)FindResource("SelectedTableShadow");
        }
        else
        {
            border.BorderThickness = new Thickness(1);
            border.Effect = null;

            var dto = DataContext as TableDto;
            if (dto != null)
                ApplyStatusStyle(dto.Status);
        }
    }

    private void ApplyStatusStyle(string status)
    {
        switch (status)
        {
            case TableStatus.Free:
                border.Background = (Brush)FindResource("FreeTableBackground");
                border.BorderBrush = (Brush)FindResource("FreeTableBorderBrush");
                txtStatus.Foreground = (Brush)FindResource("FreeTableForeground");
                break;

            case TableStatus.Busy:
                border.Background = (Brush)FindResource("BusyTableBackground");
                border.BorderBrush = (Brush)FindResource("BusyTableBorderBrush");
                txtStatus.Foreground = (Brush)FindResource("BusyTableForeground");
                break;

            case TableStatus.Reserved:
                border.Background = (Brush)FindResource("ReservedTableBackground");
                border.BorderBrush = (Brush)FindResource("ReservedTableBorderBrush");
                txtStatus.Foreground = (Brush)FindResource("ReservedTableForeground");
                break;
        }
    }
}
