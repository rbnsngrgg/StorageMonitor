using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Timers;

namespace RectImagesMonitor
{
    class MonitorService : ServiceBase
    {
        private string LogLocation { get; set; }
        private List<DriveMonitor> Monitors { get; set; }
        private Timer Timer { get; set; }
        private List<String> LogLines { get; set; } = new List<string>();
        public bool Verbose { get; set; } = false;
        public MonitorService(List<DriveMonitor> monitors, int timeInterval, bool verbose, string exePath) : base()
        {
            LogLocation = Path.Join(exePath, "MonitorLog.txt");
            Monitors = monitors;
            Timer = new Timer(timeInterval);
            Timer.Elapsed += CheckDrives;
            Timer.AutoReset = true;
            Timer.Enabled = true;
            Verbose = verbose;
        }

        public void Log(string message, int count = 0)
        {
            try
            {
                File.AppendAllText(LogLocation, $"{Timestamp()}: {message}\n");
            }
            catch(System.IO.IOException)
            {
                if(count < 5)
                {
                    System.Threading.Thread.Sleep(100);
                    Log(message, count + 1);
                }
                else
                {
                    Environment.Exit(1);
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            Log("Starting service");
            string message = $"Monitoring drives with settings:\n\tTimeIntervalSeconds = {Timer.Interval / 1000}";
            foreach(DriveMonitor monitor in Monitors)
            { message += monitor.ToString(); }
            Log(message);
            Timer.Start();
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            Log("Stopping service");
            Timer.Stop();
            base.OnStop();
        }

        protected override void OnPause()
        {
            Log("Pausing service");
            base.OnPause();
        }

        protected override void OnContinue()
        {
            Log("Resuming service");
            base.OnContinue();
        }

        protected void CheckDrives(Object source, ElapsedEventArgs e)
        {
            Log($"Checking {Monitors.Count} drive(s) -----------------------------------------------------------");
            foreach (DriveMonitor monitor in Monitors)
            {
                Log($"Checking drive: {monitor.Drive} -----------------------------------------------------------");
                CheckStorage(monitor);
            }
        }

        protected void CheckStorage(DriveMonitor monitor)
        {
            foreach(DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.Name == monitor.Drive)
                {
                    long available = drive.AvailableFreeSpace;
                    long totalSpace = drive.TotalSize;
                    double percent = (double)available / totalSpace;
                    Log($"\n\tTotal space = {totalSpace}\n\tAvailable space = {available} ({percent*100}%)");
                    if ( percent  < monitor.TriggerPercent || (monitor.TriggerPercent < 0))
                    {
                        Log($"Clearing files and folders from {monitor.RemoveFilesFrom} older than {monitor.FilesOlderThanDays} days");
                        ClearStorage(monitor);
                    }
                    return;
                }
            }
            Log($"Drive {monitor.Drive} not found");
        }

        protected void ClearStorage(DriveMonitor monitor)
        {
            try
            {
                foreach (string folder in Directory.GetDirectories(monitor.RemoveFilesFrom))
                {
                    double folderAgeDays = (DateTime.UtcNow - Directory.GetLastWriteTime(folder)).TotalDays;
                    if (Verbose)
                    {
                        Log("\n\t\t(verbose logging):" +
                            $"\n\t\t\tFolder {folder} last write time: {Directory.GetLastWriteTime(folder)}" +
                            $"\n\t\t\t\tFolder is {folderAgeDays} days old." +
                            $"\n\t\t\t\tDelete (older than {monitor.FilesOlderThanDays} days): {folderAgeDays >= monitor.FilesOlderThanDays}");
                    }
                    if (folderAgeDays >= monitor.FilesOlderThanDays)
                    {
                        try
                        {
                            if (monitor.DisposalMethod == "Delete")
                            {
                                if (Verbose)
                                {
                                    Log($"\t\t\t\tDeleting {folder}");
                                }
                                if (folder.EndsWith(" ")) { Directory.Delete($@"{folder}\", true); }
                                else { Directory.Delete(folder, true); }
                            }
                            else
                            {
                                if (Verbose)
                                {
                                    Log($"\t\t\t\tMoving {folder} to {monitor.FileMoveLocation}");
                                }
                                Directory.Move(folder, monitor.FileMoveLocation);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log(ex.Message);
                        }
                    }
                }
                foreach (string file in Directory.GetFiles(monitor.RemoveFilesFrom))
                {
                    double fileAgeDays = (DateTime.UtcNow - Directory.GetLastWriteTime(file)).TotalDays;
                    if (Verbose)
                    {
                        Log("\n\t\t(verbose logging):" +
                            $"\n\t\t\tFile {fileAgeDays} last write time: {Directory.GetLastWriteTime(file)}" +
                            $"\n\t\t\t\tFile is {fileAgeDays} days old." +
                            $"\n\t\t\t\tDelete (older than {monitor.FilesOlderThanDays} days): {fileAgeDays >= monitor.FilesOlderThanDays}");
                    }
                    if (fileAgeDays >= monitor.FilesOlderThanDays)
                    {
                        try
                        {
                            if (monitor.DisposalMethod == "Delete")
                            {
                                if (Verbose)
                                {
                                    Log($"\t\t\t\tDeleting {file}");
                                }
                                File.Delete(file);
                            }
                            else
                            {
                                if (Verbose)
                                {
                                    Log($"\t\t\t\tMoving {file} to {monitor.FileMoveLocation}");
                                }
                                File.Move(file, monitor.FileMoveLocation);
                            }
                        }
                        catch(Exception ex)
                        {
                            Log(ex.Message);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log(ex.Message);
            }
        }
        protected string Timestamp()
        {
            return DateTime.UtcNow.ToString("u");
        }
    }
}
