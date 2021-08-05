namespace RectImagesMonitor
{
    public class DriveMonitor
    {
        //"\t<DriveMonitor Drive = \"H:\\\" RemoveFilesFrom = \"\" DisposalMethod = \"Delete\" FilesOlderThanDays = \"30\" FileMoveLocation = \"\" " +
        //"TriggerPercent = \"90\"/>\n" +
        public string Drive { get; set; }
        public string RemoveFilesFrom { get; set; }
        public string DisposalMethod { get; set; }
        public int FilesOlderThanDays { get; set; }
        public string FileMoveLocation { get; set; }
        public float TriggerPercent { get; set; }

        public override string ToString()
        {
            string text = $"\n\tDrive: {Drive}" +
                $"\n\tRemoveFilesFrom: {RemoveFilesFrom}" +
                $"\n\tDisposalMethod: {DisposalMethod}" +
                $"\n\tFilesOlderThanDays: {FilesOlderThanDays}" +
                $"\n\tFileMoveLocation: {FileMoveLocation}" +
                $"\n\tTriggerPercent: {TriggerPercent}\n";
            return text;
        }
    }
}
