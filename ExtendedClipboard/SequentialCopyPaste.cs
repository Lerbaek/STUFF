using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Logging;
using WK.Libraries.SharpClipboardNS;
using static System.Environment;
using static System.StringSplitOptions;
using static Logging.Severity;
using static WK.Libraries.SharpClipboardNS.SharpClipboard;

namespace ExtendedClipboard
{
  public class SequentialCopyPaste
  {
    private static SequentialCopyPaste _instance;

    private readonly ILogger _logger;
    private SharpClipboard _clipboard;
    private IKeyboardMouseEvents _keyboardHook;
    private  Queue<string> _cache;

    public bool Active { get; private set; }

    private SequentialCopyPaste(ILogger logger)
      => _logger = logger;

    public static SequentialCopyPaste GetInstance(ILogger logger) =>
      _instance ?? (_instance = new SequentialCopyPaste(logger));

    private void   RegisterClipboardChanged() =>
      _clipboard.ClipboardChanged += ClipboardChanged;

    private void UnregisterClipboardChanged() =>
      _clipboard.ClipboardChanged -= ClipboardChanged;

    public bool Toggle()
    {
      _logger.Log(Info, $"{(Active ? "Dea" : "A")}ctivating sequential copy paste...");
      if (Active)
        Stop();
      else
        Start();
      return Active = !Active;
    }

    private void Start()
    {
      _cache = new Queue<string>();
      Clipboard.Clear();
      _clipboard = new SharpClipboard();
      RegisterClipboardChanged();

      _keyboardHook = Hook.GlobalEvents();
      _keyboardHook.OnCombination(new []{new KeyValuePair<Combination, Action>(Combination.TriggeredBy(Keys.V).Control(), PasteEvent)});

      _logger.Log(Info, "Sequential copy paste has been started. Use Ctrl+V to paste.");
    }

    private void Stop()
    {
      UnregisterClipboardChanged();

      _clipboard.Dispose();
      _keyboardHook.Dispose();

      _logger.Log(Info, "Sequential copy paste has been stopped.");
    }

    private void PasteEvent()
    {
      UnregisterClipboardChanged();
      if(_cache.Any())
      {
        Clipboard.SetText(_cache.Dequeue());
        _logger.Log(Info, $"Clipboard content set to \"{Clipboard.GetText()}\".");
      }
      else
      {
        Clipboard.Clear();
        _logger.Log(Info, "Clipboard empty.");
      }
      RegisterClipboardChanged();
    }

    private void ClipboardChanged(object sender, ClipboardChangedEventArgs e)
    {
      if (e.ContentType != ContentTypes.Text) return;
      var content = (string) e.Content;

      _logger.Log(Info, $"Clipboard entry has been added to queue:{NewLine}{(string)e.Content}");
      foreach (var line in content.Split(new[] { NewLine }, RemoveEmptyEntries))
        _cache.Enqueue(line);
    }
  }
}
