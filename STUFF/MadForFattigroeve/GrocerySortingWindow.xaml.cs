using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Logging;
using MadForFattigroeve;

namespace STUFF.MadForFattigroeve
{
  /// <summary>
  /// Interaction logic for GrocerySortingWindow.xaml
  /// </summary>
  public partial class GrocerySortingWindow : Window
  {
    private const string DragSource = "DragSource";
    public ObservableCollection<string> UnsortedGroceries { get; }
    public ObservableCollection<string> SortedGroceries { get; }

    public GrocerySortingWindow()
    {
      SortedGroceries = new ObservableCollection<string>();
      UnsortedGroceries = new ObservableCollection<string>(
        new ShoppingList(new WPFLogger()).NewestShoppingList
                                         .Select(g => g.Split(new[] {" = "}, StringSplitOptions.RemoveEmptyEntries).First())
                                         .Where (g => !SortedGroceries.Contains(g)));
      InitializeComponent();
    }

    private void GroceryListViewItem_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed || e.Source == null)
        return;

      var groceryListViewItem = (ListViewItem)sender;
      var dataObject = new DataObject(groceryListViewItem.Content);
      var listView = groceryListViewItem.GetListView();
      dataObject.SetData(DragSource, listView.ItemsSource);
      DragDrop.DoDragDrop(listView, dataObject, DragDropEffects.Move);
    }

    private void GroceryListViewItem_DragOver(object sender, DragEventArgs e)
    {
      var listViewItem = (ListViewItem)sender;
      var listView = listViewItem.GetListView();
      var data = (string)e.Data.GetData(DataFormats.Text);
      if (data == listViewItem.Content as string)
        return;

      var source = (ObservableCollection<string>) e.Data.GetData(DragSource);
      var target = (ObservableCollection<string>) listView.ItemsSource;
      var index = target.IndexOf(listViewItem.Content as string);
      source.Remove(data);
      target.Insert(index, data);
      listView.SelectedIndex = index;
      listView.ScrollIntoView(data);
      e.Data.SetData(DragSource, target);
    }

    private void GroceryListView_DragOver(object sender, DragEventArgs e)
    {
      var listView = (ListView)sender;
      var data = (string)e.Data.GetData(DataFormats.Text);
      var source = (ObservableCollection<string>)e.Data.GetData(DragSource) ?? throw new NullReferenceException("Drag source not provided");
      var target = (ObservableCollection<string>)listView.ItemsSource;
      if (!target.Contains(data))
      {
        source.Remove(data);
        target.Add(data);
        listView.SelectedIndex = target.Count - 1;
        e.Data.SetData(DragSource, target);
      }

      listView.UpdateLayout();
      var scrollViewer = FindVisualChild<ScrollViewer>(listView);

      const double tolerance = 10;
      const double offset = 1;
      var verticalPos = e.GetPosition(listView).Y;

      if (verticalPos < tolerance) // Top of visible list?
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset); // Scroll up.

      else if (verticalPos > listView.ActualHeight - tolerance) // Bottom of visible list?
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset); // Scroll down.    
    }

    public static TChildItem FindVisualChild<TChildItem>(DependencyObject obj) where TChildItem : DependencyObject
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
