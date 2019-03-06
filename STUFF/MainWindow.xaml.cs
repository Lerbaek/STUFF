using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ExtendedClipboard;
using Logging;
using Microsoft.Win32;
using static System.Environment;
using static Logging.Severity;

namespace STUFF
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public ILogger Logger { get; }
    public IEnumerable<Button> Buttons => ButtonsStackPanel.Children.OfType<Button>();
    public SequentialCopyPaste SequentialCopyPaste { get; }

    public MainWindow()
    {
      Logger = new WPFLogger();
      SequentialCopyPaste = SequentialCopyPaste.GetInstance(Logger);
      InitializeComponent();
      ((INotifyCollectionChanged) LogDataGrid.Items).CollectionChanged += LogDataGrid_SourceUpdated;
    }

    private void SequentialCopyPasteButton_Click(object sender, RoutedEventArgs e)
    {
      Logger.Log(Info, "Sequential Copy Paste was clicked.");
      SequentialCopyPaste.Toggle();
      foreach (var button in Buttons.Where(b => !ReferenceEquals(b, sender)))
        button.IsEnabled = !SequentialCopyPaste.Active;
    }

    private void LogDataGrid_SourceUpdated(object sender, NotifyCollectionChangedEventArgs e)
    {
      var lastRowIndex = LogDataGrid.Items.Count - 1;
      var lastRowItem = LogDataGrid.Items.GetItemAt(lastRowIndex);
      LogDataGrid.ScrollIntoView(lastRowItem);
    }

    private void TimestampRenameButton_Click(object sender, RoutedEventArgs e)
    {
      const string imageExtensions = "*.gif;*.jpg;*.jpeg;*.png;*.tif";
      const string videoExtensions = "*.3gp;*.avi;*.m4v;*.mkv;*.mp4;*.mov;*.mts;*.wmv";
      var dialog = new OpenFileDialog
      {
        Title = "Select files to rename",
        DereferenceLinks = true,
        Filter =    "All Files|*.*" +
                $"|Media Files|{imageExtensions};{videoExtensions}" +
                $"|Image Files|{imageExtensions}" +
                $"|Video Files|{videoExtensions}",
        FilterIndex = 2,
        Multiselect = true,
      };
      dialog.FileOk += TimestampRenameFiles;
      dialog.ShowDialog();
    }

    private void TimestampRenameFiles(object sender, CancelEventArgs e)
    {
      OpenFileDialog dialog = (OpenFileDialog) sender;
      Logger.Log(Info, $"Files selected for renaming:{NewLine}{dialog.FileNames.Aggregate((c, n) => $"{c}{NewLine}{n}")}");
    }
  }
}
