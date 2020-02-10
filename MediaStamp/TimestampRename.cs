using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using MetadataExtractor;
using STUFF.Logging;
using STUFF.Logging.Properties;
using static System.Environment;
using static System.IO.File;
using static System.IO.Path;
using static STUFF.Logging.Severity;
using Directory = MetadataExtractor.Directory;

namespace STUFF.MediaStamp
{
  public class TimestampRename : INotifyPropertyChanged
  {
    private readonly ILogger logger;
    private bool dryRun;
    private TimeSpan? offset;

    public bool DryRun
    {
      get => dryRun;
      set
      {
        dryRun = value;
        OnPropertyChanged();
      }
    }

    public TimeSpan Offset
    {
      get => offset ?? TimeSpan.Zero;
      set
      {
        offset = value;
        OnPropertyChanged();
      }
    }

    public TimestampRename(ILogger logger, bool dryRun = false)
    {
      this.logger = logger;
      DryRun = dryRun;
    }

    public void Rename(IEnumerable<string> files)
    {
      foreach(var file in files)
        Rename(file);
    }

    public void Rename([NotNull]string file)
    {
      try
      {
        var dir = ImageMetadataReader.ReadMetadata(file).FirstOrDefault(d => d.Tags.Any(HasDateTimeOriginalTag));
        var timestamp = (dir != null ? GetTimestampFromMetadata(dir) : GetCreationTime(file)).Add(Offset);

        var oldPath = System.IO.Directory.GetParent(file).FullName;
        var extension = GetExtension(file);
        var newFile = Combine(oldPath, timestamp.ToString("yyyy-MM-dd HH.mm.ss")) + extension;
        if (newFile == file)
        {
          logger.Log(Info, $"Skipping {file}, file name already timestamp.");
          return;
        }
        var i = 1;

        var newFileName = GetFileNameWithoutExtension(newFile);
        while (Exists(newFile))
          newFile = Combine(
            GetDirectoryName(newFile) ?? throw new InvalidOperationException($"Could not find the directory name of {newFile}"),
            $"{newFileName}-{i++}{extension}");

        logger.Log(Info, (DryRun ? $"[DRY RUN]{NewLine}" : "") +
                                    $"Old file: {file}{NewLine}" +
                                    $"New file: {newFile}");

        if(!DryRun)
          Move(file, newFile);
      }
      catch(Exception e)
      {
        logger.Log(Error, $"An error occurred for {file}:{NewLine}" +
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
