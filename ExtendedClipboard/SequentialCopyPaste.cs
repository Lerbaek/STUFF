using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Logging;
using Logging.Annotations;
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

    public bool Active
    {
      get => _active;
      private set
      {
        if (value == _active) return;
        _active = value;
        OnPropertyChanged();
      }
    }

    public bool AddNewClipboardEntriesToQueue { private get; set; }

    public bool CountPrefix { private get; set; }
    public bool StopWhenEmpty { private get; set; }

    private int _count = 0;
    private bool _active;

    private SequentialCopyPaste(ILogger logger)
    {
      _logger = logger;
      AddNewClipboardEntriesToQueue = true;
      CountPrefix = true;
    }

    public static SequentialCopyPaste GetInstance(ILogger logger) => _instance ?? (_instance = new SequentialCopyPaste(logger));

    private void   RegisterClipboardChanged() => _clipboard.ClipboardChanged += ClipboardChanged;

    private void UnregisterClipboardChanged() => _clipboard.ClipboardChanged -= ClipboardChanged;

    public bool Toggle()
    {
      _logger.Log(Info, $"{(Active ? "Dea" : "A")}ctivating sequential copy paste...");
      return Active ? Stop() : Start();
    }

    private bool Start()
    {
      _cache = new Queue<string>();
      Clipboard.Clear();
      _clipboard = new SharpClipboard();
      RegisterClipboardChanged();

      _keyboardHook = Hook.GlobalEvents();
      _keyboardHook.OnCombination(new []{new KeyValuePair<Combination, Action>(Combination.TriggeredBy(Keys.V).Control(), PasteEvent)});

      _logger.Log(Info, "Sequential copy paste has been started. Use Ctrl+V to paste.");

      return Active = true;
    }

    private bool Stop()
    {
      UnregisterClipboardChanged();

      _clipboard.Dispose();
      _keyboardHook.Dispose();

      _logger.Log(Info, "Sequential copy paste has been stopped.");

      return Active = false;
    }

    private void PasteEvent()
    {
      lock(_cache)
      {
        UnregisterClipboardChanged();
        if(_cache.Any())
        {
          ++_count;
          Clipboard.SetText((CountPrefix ? $"{_count.ToString().PadLeft(2, '0')}: " : string.Empty) +  _cache.Dequeue());
          _logger.Log(Info, $"Clipboard content set to \"{Clipboard.GetText()}\".");
        }
        else
        {
          Clipboard.Clear();
          _logger.Log(Info, "Clipboard empty.");
          if (StopWhenEmpty)
            Stop();
        }
      }
      RegisterClipboardChanged();
    }

    private void ClipboardChanged(object sender, ClipboardChangedEventArgs e)
    {
      if (e.ContentType != ContentTypes.Text) return;
      var content = (string) e.Content;
      var newEntries = content.Split(new[] { NewLine }, RemoveEmptyEntries);

      if(AddNewClipboardEntriesToQueue)
        foreach (var line in newEntries)
          _cache.Enqueue(line);

      else
      {
        _cache = new Queue<string>(newEntries);
        _count = 0;
      }

      _logger.Log(Info, $"Clipboard entries have been {(AddNewClipboardEntriesToQueue ? "added to queue" : "set")}:{NewLine}{(string)e.Content}");
    }

    #region Event handler

    public static event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}
