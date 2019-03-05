using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ExtendedClipboard;
using Logging;
using static Logging.Severity;

namespace STUFF
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public ILogger Logger { get; }
    public IEnumerable<Button> Buttons => Grid.Children.OfType<Button>();
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
  }
}
