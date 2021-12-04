using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TagLib;

namespace NCMDumpCore
{
    public class NCMDump
    {
        readonly byte[] coreKey = { 0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57 };
        readonly byte[] metaKey = { 0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28 };

        private bool ReadHeader(ref MemoryStream ms)
        {
            byte[] header = new byte[8];
            // Header Should be "CTENFDAM"
            ms.Read(header, 0, header.Length);
            return Encoding.ASCII.GetString(header) == "CTENFDAM";
        }

        private byte[] MakeKeybox(ref MemoryStream ms)
        {
            // read keybox length
            uint KeyboxLength = ReadUint32(ms);
            //Console.WriteLine($"AES Key Length: {KeyboxLength}");

            // read raw keybox data
            var buffer = new byte[KeyboxLength];
            ms.Read(buffer, 0, buffer.Length);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] ^= 0x64;
            }

            // decrypt keybox data
            using (Aes aes = Aes.Create())
            {
                aes.Key = coreKey;
                aes.Mode = CipherMode.ECB;
                var decrypter = aes.CreateDecryptor();
                // 17 = len("neteasecloudmusic")
                buffer = decrypter.TransformFinalBlock(buffer, 0, buffer.Length).Skip(17).ToArray();
            }
            byte[] Keybox = Enumerable.Range(0, 256).Select(s => (byte)s).ToArray();
            byte c = 0;
            byte lastbyte = 0;
            byte offset = 0;
            for (int i = 0; i < Keybox.Length; i++)
            {
                var swap = Keybox[i];
                c = (byte)((swap + lastbyte + buffer[offset]) & 0xff);
                offset++;
                if (offset >= buffer.Length)
                {
                    offset = 0;
                }
                Keybox[i] = Keybox[c];
                Keybox[c] = swap;
                lastbyte = c;
            }

            return Keybox;
        }

        private MetaInfo ReadMeta(ref MemoryStream ms)
        {
            // read meta length
            var MetaLength = ReadUint32(ms);
            var RawMetaData = new byte[MetaLength];
            ms.Read(RawMetaData, 0, RawMetaData.Length);
            for (int i = 0; i < RawMetaData.Length; i++)
            {
                RawMetaData[i] ^= 0x63;
            }
            RawMetaData = System.Convert.FromBase64String(Encoding.ASCII.GetString(RawMetaData.Skip(22).ToArray()));

            // decrypt meta data which is a json contains info of the song
            using (Aes aes = Aes.Create())
            {
                aes.Key = metaKey;
                aes.Mode = CipherMode.ECB;
                var decrypter = aes.CreateDecryptor();
                RawMetaData = decrypter.TransformFinalBlock(RawMetaData, 0, RawMetaData.Length);
                var MetaJsonString = Encoding.UTF8.GetString(RawMetaData).Replace("music:", "");
                MetaInfo metainfo = new MetaInfo();

                metainfo = JsonSerializer.Deserialize<MetaInfo>(MetaJsonString);

                //// Inverse to json and verify

                //var options = new JsonSerializerOptions
                //{
                //    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                //    WriteIndented = true
                //};
                //Console.WriteLine(JsonSerializer.Serialize(metainfo, options));

                return metainfo;
            }
        }

        private byte[] ReadAudioData(ref MemoryStream ms, byte[] key_box)
        {
            byte[] chunk = new byte[0x8000];
            int j;
            using (MemoryStream msData = new MemoryStream())
            {
                while (true)
                {
                    if (ms.Read(chunk, 0, chunk.Length) <= 0)
                    {
                        break;
                    }
                    for (int i = 0; i < chunk.Length; i++)
                    {
                        j = (i + 1) & 0xff;
                        chunk[i] ^= key_box[(key_box[j] + key_box[(key_box[j] + j) & 0xff]) & 0xff];
                    }
                    msData.Write(chunk, 0, chunk.Length);
                }
                return msData.ToArray();
            }
        }

        private void AddTag(string fileName, byte[] ImgData, MetaInfo metainfo)
        {

            var tagfile = TagLib.File.Create(fileName);
            AddCover(tagfile);

            //Add more infomation
            tagfile.Tag.Title = metainfo.musicName;
            tagfile.Tag.Performers = metainfo.artist.Select(x => x[0].GetString()).ToArray();
            tagfile.Tag.Album = metainfo.album;
            tagfile.Tag.Subtitle = String.Join(@";", metainfo.alias);
            tagfile.Save();


            void AddCover(TagLib.File tagfile)
            {
                //Use Embedded Picture
                if (ImgData.Length != 0)
                {
                    var PicEmbedded = new Picture(new ByteVector(ImgData));
                    tagfile.Tag.Pictures = new Picture[] { PicEmbedded };
                }
                //Use Internet Picture
                else if (metainfo.albumPic != "")
                {
                    byte[] NetImgData;
                    NetImgData = FetchUrl(new Uri(metainfo.albumPic));
                    if (NetImgData.Length != 0)
                    {
                        var PicFromNet = new Picture(new ByteVector(NetImgData));
                        tagfile.Tag.Pictures = new Picture[] { PicFromNet };
                    }
                }
                tagfile.Save();
            }
        }

        private byte[] FetchUrl(Uri uri)
        {
            HttpClient client = new HttpClient();
            try
            {
                var response = client.GetAsync(uri).Result;
                Console.WriteLine(response.StatusCode);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = response.Content.ReadAsStream())
                    {
                        using (var memStream = new MemoryStream())
                        {
                            stream.CopyTo(memStream);
                            memStream.Position = 0;
                            Console.WriteLine("album picture Load OK : remote returned {0}", response.StatusCode);
                            return memStream.ToArray();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Failed to download album picture: remote returned {0}", response.StatusCode);
                    return null;
                }

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }

        private uint ReadUint32(MemoryStream ms)
        {
            byte[] buffer = new byte[4];
            ms.Read(buffer, 0, buffer.Length);
            return BitConverter.ToUInt32(buffer);
        }

        public async Task<bool> ConvertAsync(string path)
        {
            return Convert(path);
        }

        public bool Convert(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("File Path Not Exist!");
                return false;
            }

            //Read all bytes to ram.
            var ms = new MemoryStream(System.IO.File.ReadAllBytes(path));

            //Verify Header
            if (!ReadHeader(ref ms))
            {
                Console.WriteLine("Not a NCM File");
                return false;
            }
            // skip 2 bytes
            ms.Seek(2, SeekOrigin.Current);

            //Make Keybox
            byte[] KeyBox = MakeKeybox(ref ms);

            //Read Meta Info
            MetaInfo metainfo = ReadMeta(ref ms);
            string format = metainfo.format;

            //CRC32 Check
            var crc32bytes = new byte[4];
            ms.Read(crc32bytes, 0, crc32bytes.Length);
            var crc32len = BitConverter.ToInt32(crc32bytes);

            // skip 5 character, 
            ms.Seek(5, SeekOrigin.Current);

            // read image length
            var ImageLength = ReadUint32(ms);

            // read image data
            byte[] ImageData = new byte[ImageLength];
            ms.Read(ImageData, 0, ImageData.Length);

            // Read Audio Data
            byte[] AudioData = ReadAudioData(ref ms, KeyBox);

            //Flush Audio Data to disk drive
            string OutputPath = path.Substring(0, path.LastIndexOf("."));

            if (format is null or "") format = "mp3";
            System.IO.File.WriteAllBytes($"{OutputPath}.{format}", AudioData);

            //Add tag and cover
            AddTag($"{OutputPath}.{format}", ImageData, metainfo);

            return true;
        }

    }
}


