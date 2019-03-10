using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MadForFattigroeve.Test
{
  public class ShoppingListTests
  {
    private ShoppingList _uut;

    private readonly KeyValuePair<string, IEnumerable<string>> _expectedShoppingList
      = new KeyValuePair<string, IEnumerable<string>>(
          "https://madforfattigroeve.dk/2019/03/2019-madplan-for-uge-11/", new[]
    {
      "1 kg. gulerødder = 6,50 kr.",
      "2 x squash = 10 kr. (Tilbud)",
      "2 kg. kartofler, løsvægt = 15 kr.",
      "2 x broccoli = 16 kr. (Tilbud)",
      "Frisk spinat, 75 g. = 9 kr.",
      "Schulstad rugbrød = 12 kr.",
      "Baguettes = 3,95 kr.",
      "Æg = 23,50 kr.",
      "Revet cheddar = 11,95 kr.",
      "2 x bacon i skiver = 17,90 kr.",
      "1 x bacon i tern = 10,95 kr.",
      "450 g. kyllingebrystfilet = 26,50 kr.",
      "Hele kyllingelår, 1,5 kg. = 20 kr. (Tilbud)",
      "450 g. hk. oksefars = 20 kr. (Tilbud)",
      "Bønner, frost = 10,50 kr.",
      "Garli flødeost = 8,25 kr.",
      "Mælk = 7,50 kr.",
      "Frisk mozzarella = 5,95 kr.",
      "Fløde, OMA = 7,95 kr.",
      "Kyllingebrystfilet, pålæg = 10 kr. (Tilbud)",
      "Smør = 12,50 kr.",
      "Ris = 4,50 kr.",
      "Lasagneblanding = 14,95 kr.",
      "Bearnaisesauce = 9,95 kr.",
      "Løg + hvidløg",
      "Salt + peber",
      "Olie",
      "Mel",
      "Bagepulver",
      "Solsikkekerner",
      "Oksebouillon",
      "Grøntsagsbouillon",
      "Sukker",
      "Rødløg",
      "Oregano",
      "Citronsaft",
      "Herbes de provence",
      "Mayonnaise"
    });

    [SetUp]
    public void Setup() => _uut = new ShoppingList();

    [Test]
    public void DoStuff() => Assert.That(_uut.GetShoppingList(_expectedShoppingList.Key), Is.EquivalentTo(_expectedShoppingList.Value));
  }
}
