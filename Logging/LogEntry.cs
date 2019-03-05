using System;

namespace Logging
{
  class LogEntry : ILogEntry
  {
    public LogEntry(Severity severity, string message)
    {
      TimeStamp = DateTime.Now;
      Severity = severity;
      Message = message;
    }

    private DateTime TimeStamp { get; }
    private Severity Severity { get; }
    private string Message { get; }
    public string Entry => $"[{TimeStamp:HH:mm:ss}] - [{Severity}] - {Message}";
  }
}