using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ExtendedClipboard;
using Logging;
using MadForFattigroeve;
using MediaStamp;
using Microsoft.Win32;
using STUFF.MadForFattigroeve;
using static System.Windows.Visibility;
using static Logging.Severity;
using static STUFF.MainWindow.StartButtonStates;

namespace STUFF
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public ILogger Logger { get; }
    public IEnumerable<Button> Buttons => ButtonsDockPanel.Children.OfType<Button>();
    public IEnumerable<StackPanel> OptionsStackPanels => OptionsGrid.Children.OfType<StackPanel>();
    public SequentialCopyPaste SequentialCopyPaste { get; }
    public TimestampRename TimestampRename { get; }

    public enum StartButtonStates
    {
      Start,
      Stop
    }

    public MainWindow()
    {
      Logger = new WPFLogger();
      SequentialCopyPaste = SequentialCopyPaste.GetInstance(Logger);
      TimestampRename = new TimestampRename(Logger);
      InitializeComponent();
      ((INotifyCollectionChanged) LogDataGrid.Items).CollectionChanged += LogDataGrid_SourceUpdated;
    }

    private void SequentialCopyPasteButton_Click(object sender, RoutedEventArgs e) => ShowOptions(SequentialCopyPasteOptionsStackPanel);

    private void LogDataGrid_SourceUpdated(object sender, NotifyCollectionChangedEventArgs e)
    {
      var lastRowIndex = LogDataGrid.Items.Count - 1;
      var lastRowItem = LogDataGrid.Items.GetItemAt(lastRowIndex);
      LogDataGrid.ScrollIntoView(lastRowItem);
    }

    private void TimestampRenameButton_Click(object sender, RoutedEventArgs e) => ShowOptions(TimestampRenameOptionsStackPanel);

    private void SetButtonStates(bool state, Button startButton)
    {
      foreach (var button in Buttons)
        button.IsEnabled = state;
      startButton.Content = state ? Start : Stop;
    }

    private void ShowOptions(StackPanel optionsStackPanel)
    {
      foreach (var stackPanel in OptionsStackPanels.Where(b => !ReferenceEquals(b, optionsStackPanel)))
        stackPanel.Visibility = Collapsed;
      optionsStackPanel.Visibility = Visible;
    }

    private void TimestampRenameStartButton_Click(object sender, RoutedEventArgs e)
    {
      const string imageExtensions = "*.gif;*.jpg;*.jpeg;*.png;*.tif";
      const string videoExtensions = "*.3gp;*.avi;*.m4v;*.mkv;*.mp4;*.mov;*.mts;*.wmv";
      var dialog = new OpenFileDialog
      {
        Title = "Select files to rename",
        DereferenceLinks = true,
        Filter = "All Files|*.*" +
                 $"|Media Files|{imageExtensions};{videoExtensions}" +
                 $"|Image Files|{imageExtensions}" +
                 $"|Video Files|{videoExtensions}",
        FilterIndex = 2,
        Multiselect = true,
      };
      if (dialog.ShowDialog() == true)
        TimestampRename.Rename(dialog.FileNames);
    }

    private void SequentialCopyPasteStartButton_Click(object sender, RoutedEventArgs e)
    {
      Logger.Log(Info, "Sequential Copy Paste was clicked.");
      SequentialCopyPaste.Toggle();
      SetButtonStates(!SequentialCopyPaste.Active, SequentialCopyPasteStartButton);
    }

    private void AddNewToQueueCheckBox_CheckChanged(object sender, RoutedEventArgs e)
      => Logger.Log(Info,
        $"New clipboard entries will {(AddNewToQueueCheckBox.IsChecked.Value ? "be added to" : "overwrite")} the existing queue");

    private void CountPrefixCheckBox_CheckChanged(object sender, RoutedEventArgs e)
      => Logger.Log(Info,
        $"New clipboard entries will {(CountPrefixCheckbox.IsChecked.Value ? "" : "not ")}be prefixed with an incrementing number");

    private void MadForFattigroeveButton_Click(object sender, RoutedEventArgs e) =>
      ShowOptions(MadForFattigroeveOptionsStackPanel);

    private void MadForFattigroeveStartButton_Click(object sender, RoutedEventArgs e)
    {
      var button = (Button) sender;
      if (button.Content is StartButtonStates state && state == Stop)
      {
        do    { SequentialCopyPaste.Toggle();}
        while ( SequentialCopyPaste.Active );
        return;
      }
      ShoppingList shoppingList = new ShoppingList(Logger);
      shoppingList.SetSequentialClipboard();
      SetButtonStates(false, button);

      SequentialCopyPaste.PropertyChanged += (o, args) =>
      {
        var scp = (SequentialCopyPaste) o;
        if (scp.Active == false)
          SetButtonStates(true, button);
      };
    }

    private void MadForFattigroeveSortingButton_Click(object sender, RoutedEventArgs e)
    {
      new GrocerySortingWindow().ShowDialog();
    }
  }
}
