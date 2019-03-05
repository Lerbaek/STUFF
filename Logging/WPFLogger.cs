﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Logging.Annotations;

namespace Logging
{
  public class WPFLogger : ILogger
  {
    public WPFLogger() => Entries = new ObservableCollection<ILogEntry>();

    public ObservableCollection<ILogEntry> Entries { get; }
    public void Log(Severity severity, string message) => Entries.Add(new LogEntry(severity, message));
  }
}