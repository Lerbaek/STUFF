using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using STUFF.MadForFattigroeve;
using STUFF.UI.Annotations;
using static STUFF.MadForFattigroeve.GrocerySorting;

namespace STUFF.UI.MadForFattigroeve
{
  public class GrocerySortingLogic : INotifyPropertyChanged
  {
    private bool loading;

    private GrocerySortingLogic()
    {
      UnsortedGroceries = new ObservableCollection<string>();
      SortedGroceries = new ObservableCollection<string>();
      LoadGroceries();
      UnsortedGroceries.CollectionChanged += OnUnsortedGroceriesCollectionChanged;
      SortedGroceries.CollectionChanged += OnSortedGroceriesCollectionChanged;
    }

    public static GrocerySortingLogic Instance { get; } = new GrocerySortingLogic();

    public bool CanSave => !SavedData.SequenceEqual(SortedGroceries);
    public bool NewItemsAvailable => UnsortedGroceries.Any();
    public ObservableCollection<string> UnsortedGroceries { get; }
    public ObservableCollection<string> SortedGroceries { get; }


    public void LoadGroceries()
    {
      if (!File.Exists(DataFilePath))
        return;

      SortedGroceries.Clear();
      UnsortedGroceries.Clear();
      loading = true;
      foreach (var grocery in SavedData)
        SortedGroceries.Add(grocery);
      loading = false;
      LoadUnsortedGroceries();
    }

    private void LoadUnsortedGroceries()
    {
      foreach (var grocery in GroceriesThisWeek.Where(g => !SortedGroceries.Contains(g) && !UnsortedGroceries.Contains(g)))
        UnsortedGroceries.Add(grocery);
    }

    public void Save()
    {
      GrocerySorting.Save(SortedGroceries);
      LoadUnsortedGroceries();
    }

    #region Event handling

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void OnUnsortedGroceriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs args) =>
      OnPropertyChanged(nameof(NewItemsAvailable));

    private void OnSortedGroceriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if(!loading && e.Action <= NotifyCollectionChangedAction.Remove)
        LoadUnsortedGroceries();
    }

    #endregion
  }
}