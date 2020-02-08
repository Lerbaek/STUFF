using System.Windows.Controls;

namespace STUFF
{
  public static class WPFExtensions
  {
    public static ListView GetListView(this ListViewItem listViewItem) => (ListView)ItemsControl.ItemsControlFromItemContainer(listViewItem);
  }
}