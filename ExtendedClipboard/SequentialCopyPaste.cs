using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Logging;
using Logging.Annotations;
using WK.Libraries.SharpClipboardNS;
using static System.Environment;
using static System.StringSplitOptions;
using static System.Windows.Forms.Clipboard;
using static Logging.Severity;
using static WK.Libraries.SharpClipboardNS.SharpClipboard;

namespace ExtendedClipboard
{
  public class SequentialCopyPaste
  {
    #region Fields

    private bool active;

    #endregion

    #region Properties

    public bool Active
    {
      get => active;
      private set
      {
        if (value == active) return;
        active = value;
        OnPropertyChanged();
      }
    }

    public bool AddNewClipboardEntriesToQueue { private get; set; }
    public bool CountPrefix { private get; set; }
    public bool StopWhenEmpty { private get; set; }

    private int Count { get; set; }
    private IKeyboardMouseEvents KeyboardHook { get; set; }
    private ILogger Logger { get; }
    private Queue<string> Cache { get; set; }
    private SharpClipboard Clipboard { get; set; }

    private static SequentialCopyPaste Instance { get; set; }


    #endregion

    private SequentialCopyPaste(ILogger logger)
    {
      Logger = logger;
      AddNewClipboardEntriesToQueue = true;
    }

    public static SequentialCopyPaste GetInstance(ILogger logger) => Instance ?? (Instance = new SequentialCopyPaste(logger));

    private void RegisterClipboardChanged()   => Clipboard.ClipboardChanged += ClipboardChanged;
    private void UnregisterClipboardChanged() => Clipboard.ClipboardChanged -= ClipboardChanged;

    public bool Toggle()
    {
      // ReSharper disable once StringLiteralTypo
      Logger.Log(Info, $"{(Active ? "Dea" : "A")}ctivating sequential copy paste...");
      return Active ? Stop() : Start();
    }

    private void PasteEvent()
    {
      lock (Cache)
      {
        UnregisterClipboardChanged();
        if (Cache.Any())
        {
          ++Count;
          SetText((CountPrefix ? $"{Count.ToString().PadLeft(2, '0')}: " : string.Empty) + Cache.Dequeue());
          Logger.Log(Info, $"Clipboard content set to \"{GetText()}\".");
        }
        else
        {
          Clear();
          Logger.Log(Info, "Clipboard empty.");
          if (StopWhenEmpty)
            Stop();
        }
      }
      RegisterClipboardChanged();
    }

    private bool Start()
    {
      Cache = new Queue<string>();
      Clear();
      Clipboard = new SharpClipboard();
      RegisterClipboardChanged();

      KeyboardHook = Hook.GlobalEvents();
      KeyboardHook.OnCombination(new[]
        {new KeyValuePair<Combination, Action>(Combination.TriggeredBy(Keys.V).Control(), PasteEvent)});

      Logger.Log(Info, "Sequential copy paste has been started. Use Ctrl+V to paste.");

      return Active = true;
    }

    private bool Stop()
    {
      UnregisterClipboardChanged();

      Clipboard.Dispose();
      KeyboardHook.Dispose();

      Logger.Log(Info, "Sequential copy paste has been stopped.");

      return Active = false;
    }

    private void ClipboardChanged(object sender, ClipboardChangedEventArgs e)
    {
      if (e.ContentType != ContentTypes.Text)
        return;
      var content = (string)e.Content;
      var newEntries = content.Split(new[] { NewLine }, RemoveEmptyEntries);

      if (AddNewClipboardEntriesToQueue)
        foreach (var line in newEntries)
          Cache.Enqueue(line);

      else
      {
        Cache = new Queue<string>(newEntries);
        Count = 0;
      }

      Logger.Log(Info, $"Clipboard entries have been {(AddNewClipboardEntriesToQueue ? "added to queue" : "set")}:{NewLine}{content}");
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
