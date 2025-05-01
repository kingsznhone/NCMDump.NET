namespace NCMDump.Core
{
    public class MetaInfo
    {
        public string MusicId { get; set; }
        public string MusicName { get; set; }
        public List<List<string>> Artist { get; set; }
        public string AlbumId { get; set; }
        public string Album { get; set; }
        public string AlbumPicDocId { get; set; }
        public string AlbumPic { get; set; }
        public int Bitrate { get; set; }
        public string Mp3DocId { get; set; }
        public int Duration { get; set; }
        public string MvId { get; set; }
        public List<string> Alias { get; set; }
        public List<string> TransNames { get; set; }
        public string Format { get; set; }
        public float Fee { get; set; }
        public float VolumeDelta { get; set; }
        public Dictionary<string, int> Privilege { get; set; }
    }
}