using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace STUFF.UI
{
  public static class WPFExtensions
  {
    public static ListView GetListView(this ListViewItem listViewItem) => (ListView)ItemsControl.ItemsControlFromItemContainer(listViewItem);

    public static TChildItem FindVisualChild<TChildItem>(this DependencyObject obj) where TChildItem : DependencyObject
    {
      // Search immediate children first (breadth-first)
      for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
      {
        var child = VisualTreeHelper.GetChild(obj, i);

        if (child is TChildItem item)
          return item;

        var childOfChild = FindVisualChild<TChildItem>(child);

        if (childOfChild != null)
          return childOfChild;
      }

      return null;
    }
  }
}