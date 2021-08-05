using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace RectImagesMonitor
{
    public class Config
    {
        private const string configName = "MonitorConfig.xml";
        private string configLocation = "";
        public int TimeIntervalSeconds { get; private set; } = 10;
        public List<DriveMonitor> Monitors { get; private set; } = new List<DriveMonitor>();
        public List<string> Fixtures { get; private set; } = new List<string>();
        public bool Verbose { get; private set; } = false;
        private XDocument config;

        public Config(string exePath)
        {
            configLocation = Path.Join(exePath, configName);
            if (File.Exists(configLocation))
            { config = XDocument.Load(configLocation); }
            else
            { CreateConfig(); }
            LoadConfig();
        }
        private void CreateConfig()
        {
            config = XDocument.Parse(
                "<?xml version=\"1.0\" encoding=\"utf - 8\" ?>\n" +
                "<!--DisposalMethod = \"Delete\" or \"Move\" | FileMoveLocation used only with \"Move\" DisposalMethod-->\n" +
                "<MonitorConfig TimeIntervalSeconds = \"216000\" Verbose = \"false\">\n\n" +
                "\t<DriveMonitor Drive = \"H:\\\" RemoveFilesFrom = \"H:\\RectImages\" DisposalMethod = \"Delete\" FilesOlderThanDays = \"30\" FileMoveLocation = \"\" " +
                "TriggerPercent = \"10\"/>\n" +
                "</MonitorConfig>");
            config.Save(configLocation);
        }
        private bool LoadConfig()
        {
            if (config == null) { return false; }
            XDocument configDoc = XDocument.Load(configLocation);
            foreach(XElement element in configDoc.Descendants("MonitorConfig").Elements())
            {
                if (element.Name == "DriveMonitor")
                {
                    Monitors.Add(new DriveMonitor()
                    {
                        Drive = element.Attribute("Drive").Value,
                        RemoveFilesFrom = element.Attribute("RemoveFilesFrom").Value,
                        DisposalMethod = element.Attribute("DisposalMethod").Value,
                        FilesOlderThanDays = int.Parse(element.Attribute("FilesOlderThanDays").Value),
                        FileMoveLocation = element.Attribute("FileMoveLocation").Value,
                        TriggerPercent = float.Parse(element.Attribute("TriggerPercent").Value) / 100
                    });
                }
            }
            foreach(XAttribute attribute in configDoc.Descendants("MonitorConfig").Attributes())
            {
                if(attribute.Name == "TimeIntervalSeconds")
                {
                    TimeIntervalSeconds = int.Parse(attribute.Value);
                }
                else if(attribute.Name == "Verbose")
                {
                    Verbose = attribute.Value.ToLower() == "true";
                }
            }
            return true;
        }
    }
}
