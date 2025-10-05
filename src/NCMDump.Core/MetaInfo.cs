namespace NCMDump.Core
{
    public class MetaInfo
    {
        public string MusicId { get; set; } = "";
        public string MusicName { get; set; } = "";
        public List<List<string>> Artist { get; set; } = new List<List<string>>();
        public string AlbumId { get; set; } = "";
        public string Album { get; set; } = "";
        public string AlbumPicDocId { get; set; } = "";
        public string AlbumPic { get; set; } = "";
        public int Bitrate { get; set; } = 0;
        public string Mp3DocId { get; set; } = "";
        public int Duration { get; set; } = 0;
        public string MvId { get; set; } = "";
        public List<string> Alias { get; set; } = new List<string>();
        public List<string> TransNames { get; set; } = new List<string>();
        public string Format { get; set; } = "";
        public float Fee { get; set; } = 0;
        public float VolumeDelta { get; set; } = 0;
        public Dictionary<string, int> Privilege { get; set; } = new Dictionary<string, int>();
    }
}