using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Logging;
using NUnit.Framework;
using Rhino.Mocks;

namespace ExtendedClipboard.Test
{
  [TestFixture]
  public class SequentialCopyPasteTests
  {
    private ILogger logger;
    private SequentialCopyPaste uut;

    [SetUp]
    public void Setup()
    {
      logger = MockRepository.GenerateMock<ILogger>();
      uut = SequentialCopyPaste.GetInstance(logger);
    }

    [TearDown]
    public void TearDown()
    {
      if (uut.Active) uut.Toggle();
    }

    private static IEnumerable<int> counts = Enumerable.Range(0, 10);

    [TestCaseSource(nameof(counts))]
    public void Toggle_AnyNumberOfTimes_ActiveIfCountUnequal(int count)
    {
      RunStaThread(() =>
      {
        for (var i = 0; i < count; i++)
          uut.Toggle();
      });
      Assert.That(uut.Active, Is.EqualTo(count % 2 == 1));
    }

    private void RunStaThread(ThreadStart action)
    {
      var thread = new Thread(action);
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();
    }
  }
}
