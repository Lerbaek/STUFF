using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ExtendedClipboard;
using Logging;
using MediaStamp;
using Microsoft.Win32;
using static Logging.Severity;

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

    private void SetButtonStates(Button excluded, bool state, Button startButton)
    {
      foreach (var button in Buttons.Where(b => !ReferenceEquals(b, excluded)))
        button.IsEnabled = state;
      startButton.Content = state ? "Start" : "Stop";
    }

    private void ShowOptions(StackPanel optionsStackPanel)
    {
      foreach (var stackPanel in OptionsStackPanels.Where(b => !ReferenceEquals(b, optionsStackPanel)))
        stackPanel.Visibility = Visibility.Collapsed;
      optionsStackPanel.Visibility = Visibility.Visible;
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
      SetButtonStates(SequentialCopyPasteButton, !SequentialCopyPaste.Active, SequentialCopyPasteStartButton);
    }

    private void CheckBox_CheckChanged(object sender, RoutedEventArgs e)
      => Logger.Log(Info,
        $"New clipboard entries will {(AddNewToQueueCheckBox.IsChecked.Value ? "be added to" : "overwrite")} the existing queue");
  }
}
