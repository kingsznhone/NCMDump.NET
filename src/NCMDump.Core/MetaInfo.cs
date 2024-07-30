namespace NCMDump.Core
{
    public class MetaInfo
    {
        public string musicId { get; set; }
        public string musicName { get; set; }
        public List<List<string>> artist { get; set; }
        public string albumId { get; set; }
        public string album { get; set; }
        public string albumPicDocId { get; set; }
        public string albumPic { get; set; }
        public int bitrate { get; set; }
        public string mp3DocId { get; set; }
        public int duration { get; set; }
        public string mvId { get; set; }
        public List<string> alias { get; set; }
        public List<string> transNames { get; set; }
        public string format { get; set; }
        public float fee { get; set; }
        public float volumeDelta { get; set; }
        public Dictionary<string, int> privilege { get; set; }
    }
}