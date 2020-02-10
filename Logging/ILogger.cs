using System.Collections.ObjectModel;

namespace STUFF.Logging
{
  public interface ILogger
  {
    ObservableCollection<ILogEntry> Entries { get; }
    void Log(Severity severity, string message);
  }
}
