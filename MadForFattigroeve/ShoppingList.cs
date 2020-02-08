using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ExtendedClipboard;
using HtmlAgilityPack;
using Logging;

namespace MadForFattigroeve
{
  public class ShoppingList
  {
    private readonly ILogger logger;

    public ShoppingList(ILogger logger) => this.logger = logger;

    private static HtmlDocument Load(string url) => new HtmlWeb().Load(url);

    private string NewestWeekPlanLink
      => Load("https://madforfattigroeve.dk/category/ugeplaner")
          .DocumentNode                                        // Document
          .Descendants("a")                                    // Link elements
          .First(a => a.InnerText.Contains("Madplan for uge")) // Latest plan link element
          .GetAttributeValue("href", null);                    // Link

    public IEnumerable<string> NewestShoppingList => GetShoppingList(NewestWeekPlanLink);

    public IEnumerable<string> GetShoppingList(string weekPlanLink)
      => Load(weekPlanLink)
          .DocumentNode                            // Document
          .Descendants("table")                    // Table elements
          .Single(t => t.HasClass("shoppinglist")) // Table with shopping list
          .Descendants("li")                       // List elements
          .Select(li => li.InnerText               // Shopping list entry
                          .TrimEnd(' ', '\r'));    // Trimmed

    public void SetSequentialClipboard() => SetSequentialClipboard(NewestShoppingList);

    public void SetSequentialClipboard(string weekPlanLink) => SetSequentialClipboard(GetShoppingList(weekPlanLink));

    public void SetSequentialClipboard(IEnumerable<string> shoppingList)
    {
      var sequentialCopyPaste = SequentialCopyPaste.GetInstance(logger);
      sequentialCopyPaste.AddNewClipboardEntriesToQueue = false;
      sequentialCopyPaste.StopWhenEmpty = true;
      if(!sequentialCopyPaste.Active)
        sequentialCopyPaste.Toggle();
      Clipboard.SetText(string.Join(Environment.NewLine, shoppingList));
    }
  }
}
