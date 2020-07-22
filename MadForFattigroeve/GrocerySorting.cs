using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using STUFF.Logging;

namespace STUFF.MadForFattigroeve
{
  public static class GrocerySorting
  {
    public static readonly string DataFolderPath = Path.Combine($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}", nameof(STUFF));
    public static readonly string DataFilePath = Path.Combine(DataFolderPath, DataFileName);
    public const string DataFileName = "GrocerySorting.xml";

    private static readonly string[] Units = { "x", @".\.", @"..\." };

    public static XmlSerializer XMLSerializer => new XmlSerializer(typeof(ObservableCollection<string>));

    public static void Save(IEnumerable<string> sortedGroceries)
    {
      if (!Directory.Exists(DataFolderPath))
        Directory.CreateDirectory(DataFolderPath);
      using (var fileStream = new FileStream(DataFilePath, FileMode.Create))
        XMLSerializer.Serialize(fileStream, sortedGroceries);
    }

    public static IEnumerable<string> SavedData
    {
      get
      {
        using (var fileStream = new FileStream(DataFilePath, FileMode.Open))
          return (IEnumerable<string>)XMLSerializer.Deserialize(fileStream);
      }
    }

    public static IEnumerable<string> GroceriesThisWeek => new ShoppingList(new WPFLogger()).NewestShoppingList.Select(TrimGroceryName);

    private static string TrimGroceryName(string groceryName)
    {
      groceryName =
        Regex.Replace(
          Regex.Replace(groceryName.Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries).First().Trim(),
            $"^[0-9]* ({string.Join("|", Units)}) ", ""), "^[0-9]* ", "");
      return char.ToUpper(groceryName[0]) + groceryName.Substring(1);
    }

    public static IEnumerable<string> SortGroceries(IEnumerable<string> shoppingList)
    {
      var sortedGroceries = SavedData.ToList();
      var evaluatedShoppingList = shoppingList as string[] ?? shoppingList.ToArray();
      var orderedMatches = evaluatedShoppingList.Where(g => sortedGroceries.Any(s => g.ToLower().Contains(s.ToLower())))
                                                .OrderBy(g => sortedGroceries.IndexOf(sortedGroceries.Where(sg => TrimGroceryName(g) == sg)
                                                                                                     .OrderBy(sg => sg.Length)
                                                                                                     .Last()));
      return orderedMatches.Concat(evaluatedShoppingList.Where(g => !sortedGroceries.Any(s => g.ToLower().Contains(s.ToLower()))));
    }
  }
}