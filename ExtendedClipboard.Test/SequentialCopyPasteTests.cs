using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Logging;
using NUnit.Framework;
using Rhino.Mocks;
using WK.Libraries.SharpClipboardNS;

namespace ExtendedClipboard.Test
{
  [TestFixture]
  public class SequentialCopyPasteTests
  {
    private ILogger _logger;
    private SequentialCopyPaste _uut;

    [SetUp]
    public void Setup()
    {
      _logger = MockRepository.GenerateMock<ILogger>();
      _uut = SequentialCopyPaste.GetInstance(_logger);
    }

    [TearDown]
    public void TearDown()
    {
      if (_uut.Active) _uut.Toggle();
    }

    private static IEnumerable<int> _counts = Enumerable.Range(0, 10);

    [TestCaseSource(nameof(_counts))]
    public void Toggle_AnyNumberOfTimes_ActiveIfCountUnequal(int count)
    {
      RunSTAThread(() =>
      {
        for (var i = 0; i < count; i++)
          _uut.Toggle();
      });
      Assert.That(_uut.Active, Is.EqualTo(count % 2 == 1));
    }

    private void RunSTAThread(ThreadStart action)
    {
      var thread = new Thread(action);
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();
    }
  }
}
