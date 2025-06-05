using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace SmartRestaurant.Desktop.Helpers.ScrollCustomization;

public static class UIElementExtensions
{
    public static void DisableTouchScrollBounce(this DependencyObject root)
    {
        foreach (var scrollViewer in FindVisualChildren<ScrollViewer>(root))
        {
            scrollViewer.ManipulationBoundaryFeedback += (s, e) => e.Handled = true;
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) yield break;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
                yield return t;

            foreach (var childOfChild in FindVisualChildren<T>(child))
                yield return childOfChild;
        }
    }
}
