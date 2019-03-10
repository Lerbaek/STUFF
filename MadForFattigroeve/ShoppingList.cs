using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace MadForFattigroeve
{
  public class ShoppingList
  {
    private static HtmlWeb HtmlWeb => new HtmlWeb();

    private string NewestWeekPlanLink
      => HtmlWeb.Load("https://madforfattigroeve.dk/category/ugeplaner")
                .DocumentNode                                        // Document
                .Descendants("a")                                    // Link elements
                .First(a => a.InnerText.Contains("Madplan for uge")) // Latest plan link element
                .GetAttributeValue("href", null);                    // Link

    public IEnumerable<string> NewestShoppingList => GetShoppingList(NewestWeekPlanLink);

    public IEnumerable<string> GetShoppingList(string weekPlanLink)
      => HtmlWeb.Load(weekPlanLink)
                .DocumentNode                            // Document
                .Descendants("table")                    // Table elements
                .Single(t => t.HasClass("shoppinglist")) // Table with shopping list
                .Descendants("li")                       // List elements
                .Select(li => li.InnerText               // Shopping list entry
                                .TrimEnd(' ', '\r'));         // Trimmed
  }
}
