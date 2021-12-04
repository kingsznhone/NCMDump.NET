using System.Text.Json;

namespace NCMDumpCore
{
    public class MetaInfo
    {
        public int musicId { get; set; }
        public string musicName { get; set; }
        public List<List<JsonElement>> artist { get; set; }
        public int albumId { get; set; }
        public string album { get; set; }
        public JsonElement albumPicDocId { get; set; }
        public string albumPic { get; set; }
        public int bitrate { get; set; }
        public string mp3DocId { get; set; }
        public int duration { get; set; }
        public int mvId { get; set; }
        public List<string> alias { get; set; }
        public string format { get; set; }
    }
}
