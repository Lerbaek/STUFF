using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using Gma.System.MouseKeyHook;
using Logging;
using Logging.Annotations;

namespace GlobalInputWatch
{
  public class SequentialCopyPaste
  {
    private readonly ILogger _logger;
    private static IKeyboardMouseEvents globalHook;
    private static SequentialCopyPaste _instance;
    public bool Active { get; private set; }

    private SequentialCopyPaste(ILogger logger)
      => _logger = logger;

    public static SequentialCopyPaste GetInstance(ILogger logger)
      => _instance ?? (_instance = new SequentialCopyPaste(logger));

    public bool Toggle()
    {
      if (Active)
        Stop();
      else
        Start();
      return Active = !Active;
    }

    private void Start()
    {
      globalHook = Hook.GlobalEvents();
      globalHook.KeyPress += GlobalHookKeyPress;
    }

    private void Stop()
    {
      globalHook.KeyPress -= GlobalHookKeyPress;
      globalHook.Dispose();
    }

    private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
    {
      if((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
      if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        switch ($"{e.KeyChar}".ToLowerInvariant())
        {
          case "c":
            _logger.Add(Severity.Info, $"Copy detected.");
            break;
          case "x":
            _logger.Add(Severity.Info, $"Cut detected.");
            break;
          case "v":
            _logger.Add(Severity.Info, $"Paste detected.");
            break;
      }
    }
  }
}
