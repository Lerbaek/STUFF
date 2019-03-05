using System.Collections.ObjectModel;

namespace Logging
{
  public interface ILogger
  {
    ObservableCollection<ILogEntry> Entries { get; }
    void Log(Severity severity, string message);
  }
}
