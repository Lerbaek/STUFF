using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static STUFF.UI.MadForFattigroeve.GrocerySortingLogic;

namespace STUFF.UI.MadForFattigroeve
{
  /// <summary>
  /// Interaction logic for GrocerySortingWindow.xaml
  /// </summary>
  public partial class GrocerySortingWindow
  {
    public GrocerySortingLogic GrocerySortingLogic { get; }

    private const string DragSource = "DragSource";

    public GrocerySortingWindow()
    {
      GrocerySortingLogic = Instance;
      InitializeComponent();
    }

    #region Event handlers

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
      // ReSharper disable once PossibleNullReferenceException
      if (source != target)
      {
        source.Remove(data);
        target.Insert(index, data);
      }
      else
        source.Move(source.IndexOf(data), index);

      listView.SelectedIndex = index;
      // ReSharper disable once AssignNullToNotNullAttribute
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
      var scrollViewer = listView.FindVisualChild<ScrollViewer>();

      const double tolerance = 10;
      const double offset = 1;
      var verticalPos = e.GetPosition(listView).Y;

      if (verticalPos < tolerance) // Top of visible list?
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset); // Scroll up.

      else if (verticalPos > listView.ActualHeight - tolerance) // Bottom of visible list?
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset); // Scroll down.    
    }

    private void GroceryListView_KeyDown(object sender, KeyEventArgs e)
    {
      var listView = (ListView)sender;
      var index = listView.SelectedIndex;
      if (e.Key == Key.Delete && index >= 0)
      {
        var content = (string)listView.SelectedItem;
        GrocerySortingLogic.SortedGroceries.Remove(content);
        SortedGroceryListView.SelectedIndex = GrocerySortingLogic.SortedGroceries.Count > index ? index : --index;
      }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (!GrocerySortingLogic.CanSave) return;
      var result = MessageBox.Show("Would you like to save changes?", "Save changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
      switch (result)
      {
        case MessageBoxResult.Yes:
          GrocerySortingLogic.Save();
          break;
        case MessageBoxResult.No:
          GrocerySortingLogic.LoadGroceries();
          break;
        case MessageBoxResult.Cancel:
          e.Cancel = true;
          break;
      }
    }

    #endregion

    #region Command bindings

    private void CommandBinding_OnSave(object sender, ExecutedRoutedEventArgs e) => GrocerySortingLogic.Save();

    private void CommandBinding_OnCanSave(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = GrocerySortingLogic.CanSave;

    private void CommandBinding_OnClose(object sender, ExecutedRoutedEventArgs e) => Close();

    #endregion
  }
}
