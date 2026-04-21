namespace NCMDump.Core
{
    public record MetaInfo
    {
        public string MusicId { get; init; } = "";
        public string MusicName { get; init; } = "";
        public List<List<string>> Artist { get; init; } = [];
        public string AlbumId { get; init; } = "";
        public string Album { get; init; } = "";
        public string AlbumPicDocId { get; init; } = "";
        public string AlbumPic { get; init; } = "";
        public int Bitrate { get; init; } = 0;
        public string Mp3DocId { get; init; } = "";
        public int Duration { get; init; } = 0;
        public string MvId { get; init; } = "";
        public List<string> Alias { get; init; } = [];
        public List<string> TransNames { get; init; } = [];
        public string Format { get; init; } = "";
        public float Fee { get; init; } = 0;
        public float VolumeDelta { get; init; } = 0;
        public Dictionary<string, int> Privilege { get; init; } = [];
    }
}
