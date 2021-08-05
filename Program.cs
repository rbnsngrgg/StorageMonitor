using System.ServiceProcess;
using System.Diagnostics;
using System.IO;
using System;
using System.Management;
namespace RectImagesMonitor
{
    class Program
    {
        public static readonly string ServiceName = "PRCPStorageMonitor";
        static void Main(string[] args)
        {
            
            //Write to system event log
            EventLog systemEventLog = new EventLog("System");
            if (!EventLog.SourceExists(ServiceName))
            {
                EventLog.CreateEventSource(ServiceName, "System");
            }
            systemEventLog.Source = ServiceName;
            systemEventLog.WriteEntry("StorageMonitor: Starting", EventLogEntryType.Information);
            //Init config and ServiceBase
            try
            {
                //Create perceptron folder in root of local drive, if it doesn't exist
                string servicePath = Path.Join(Path.GetPathRoot(Directory.GetCurrentDirectory()), "perceptron");
                Directory.CreateDirectory(servicePath);
                systemEventLog.WriteEntry($"StorageMonitor: Loading config. Location: {Path.Join(servicePath, "MonitorConfig.xml")}", EventLogEntryType.Information);
                Config config = new Config(servicePath);
                systemEventLog.WriteEntry("StorageMonitor: Starting ServiceBase", EventLogEntryType.Information);
                ServiceBase.Run(new MonitorService(config.Monitors, config.TimeIntervalSeconds * 1000, config.Verbose, servicePath));
            }
            catch(System.Exception e)
            {
                //Write exception to event log
                systemEventLog.WriteEntry(e.Message, EventLogEntryType.Error);
                Environment.Exit(2);
            }
        }
    }
}
