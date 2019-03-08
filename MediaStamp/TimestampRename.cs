using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Logging;
using Logging.Annotations;
using MetadataExtractor;
using static System.Environment;
using static Logging.Severity;
using Directory = MetadataExtractor.Directory;

namespace MediaStamp
{
  public class TimestampRename : INotifyPropertyChanged
  {
    private readonly ILogger _logger;
    private bool _dryRun;
    private TimeSpan? _offset;

    public bool DryRun
    {
      get => _dryRun;
      set
      {
        _dryRun = value;
        OnPropertyChanged();
      }
    }

    public TimeSpan? Offset
    {
      get => _offset;
      set
      {
        _offset = value;
        OnPropertyChanged();
      }
    }

    public TimestampRename(ILogger logger, bool dryRun = false)
    {
      _logger = logger;
      DryRun = dryRun;
    }

    public void Rename(IEnumerable<string> files) => Rename(files, Offset ?? TimeSpan.Zero);

    public void Rename(IEnumerable<string> files, TimeSpan offset)
    {
      foreach(var file in files)
        Rename(file, offset);
    }

    public void Rename(string file) => Rename(file, Offset ?? TimeSpan.Zero);

    public void Rename(string file, TimeSpan offset)
    {
      Offset = offset;
      try
      {
        var dir = ImageMetadataReader.ReadMetadata(file).FirstOrDefault(d => d.Tags.Any(HasDateTimeOriginalTag));
        var timestamp = (dir != null ? GetTimestampFromMetadata(dir) : File.GetCreationTime(file)).Add(Offset ?? TimeSpan.Zero);

        var oldPath = System.IO.Directory.GetParent(file).FullName;
        var newFile = Path.Combine(oldPath, timestamp.ToString("yyyy-MM-dd HH.mm.ss")) + Path.GetExtension(file);
        if (newFile == file)
        {
          _logger.Log(Info, $"Skipping {file}, file name already timestamp.");
          return;
        }
        var i = 1;

        var newFileName = Path.GetFileNameWithoutExtension(newFile);
        while (File.Exists(newFile))
          newFile = Path.Combine(Path.GetDirectoryName(newFile), $"{newFileName}-{i++}{Path.GetExtension(newFile)}");

        _logger.Log(Info, (DryRun ? $"[DRY RUN]{NewLine}" : "") +
                                    $"Old file: {file}{NewLine}" +
                                    $"New file: {newFile}");

        if(!DryRun) File.Move(file, newFile);
      }
      catch(Exception e)
      {
        _logger.Log(Error, $"An error occurred for {file}:{NewLine}" +
                           $"{e.Message}{NewLine}" +
                           $"{e.StackTrace}");
        System.Diagnostics.Debugger.Launch();
      }
    }

    private static DateTime GetTimestampFromMetadata(Directory dir)
    {
      var created = dir.Tags.First(HasDateTimeOriginalTag).Description;
      return DateTime.ParseExact(created, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private static bool HasDateTimeOriginalTag(Tag tag) => tag.Name == "Date/Time Original";

    #region Event handler

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}
