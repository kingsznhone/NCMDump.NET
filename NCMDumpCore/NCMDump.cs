using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TagLib;

namespace NCMDumpCore
{
    public class NCMDump
    {
        private readonly int vectorSize = Vector256<byte>.Count;
        private readonly byte[] coreKey = { 0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57 };
        private readonly byte[] metaKey = { 0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28 };

        private bool ReadHeader(ref MemoryStream ms)
        {
            Span<byte> header = stackalloc byte[8];
            ms.Read(header);
            // Header Should be "CTENFDAM"
            return Enumerable.SequenceEqual(header.ToArray(), new byte[] { 067, 084, 069, 078, 070, 068, 065, 077 });
        }

        private byte[] ReadRC4Key(ref MemoryStream ms)
        {
            // read keybox length
            uint KeyboxLength = ReadUint32(ref ms);

            //read raw keybox data
            Span<byte> buffer = stackalloc byte[(int)KeyboxLength];
            ref byte ref_b = ref MemoryMarshal.GetReference(buffer);
            ms.Read(buffer);

            //SIMD XOR 0x64
            Vector256<byte> xor = Vector256.Create(0x64646464).AsByte();
            int i;
            for (i = 0; i <= buffer.Length - vectorSize; i += vectorSize)
            {
                var vb = Vector256.LoadUnsafe(ref ref_b, (nuint)i);
                vb ^= xor;
                Vector256.StoreUnsafe(vb, ref ref_b, (nuint)i);
            }
            for (; i < buffer.Length; i++)
            {
                buffer[i] ^= 0x64;
            }

            // decrypt keybox data
            using (AesCng aes = new AesCng() { Key = coreKey, Mode = CipherMode.ECB })
            {
                var decrypter = aes.CreateDecryptor();
                buffer = decrypter.TransformFinalBlock(buffer.ToArray(), 0, buffer.Length).Skip(17).ToArray(); // 17 = len("neteasecloudmusic")
            }
            return buffer.ToArray();
        }

        private MetaInfo ReadMeta(ref MemoryStream ms)
        {
            // read meta length
            var MetaLength = ReadUint32(ref ms);
            Span<byte> buffer = new byte[(int)MetaLength];
            ref byte ref_b = ref MemoryMarshal.GetReference(buffer);
            ms.Read(buffer);

            //SIMD XOR 0x63
            Vector256<byte> xor = Vector256.Create(0x63636363).AsByte();
            int i;
            for (i = 0; i <= buffer.Length - vectorSize; i += vectorSize)
            {
                var vb = Vector256.LoadUnsafe(ref ref_b, (nuint)i);
                vb ^= xor;
                Vector256.StoreUnsafe(vb, ref ref_b, (nuint)i);
            }
            for (; i < buffer.Length; i++)
            {
                buffer[i] ^= 0x63;
            }

            buffer = System.Convert.FromBase64String(Encoding.ASCII.GetString(buffer.ToArray()[22..]));

            // decrypt meta data which is a json contains info of the song
            using (AesCng aes = new AesCng() { Key = metaKey, Mode = CipherMode.ECB })
            {
                var cryptor = aes.CreateDecryptor();
                buffer = cryptor.TransformFinalBlock(buffer.ToArray(), 0, buffer.Length);

                var MetaJsonString = Encoding.UTF8.GetString(buffer).Replace("music:", "");
                MetaInfo metainfo = JsonSerializer.Deserialize<MetaInfo>(MetaJsonString);
                return metainfo;
            }
        }

        private byte[] ReadAudioData(ref MemoryStream ms, byte[] Key)
        {
            using (RC4_NCM_Stream rc4s = new RC4_NCM_Stream(ms, Key))
            {
                byte[] data = new byte[ms.Length - ms.Position];
                Span<byte> buffer = new Span<byte>(data);
                rc4s.Read(buffer);
                return data;
            }
        }

        private void AddTag(string fileName, byte[]? ImgData, MetaInfo metainfo)
        {
            var tagfile = TagLib.File.Create(fileName);

            //Use Embedded Picture
            if (ImgData is not null)
            {
                var PicEmbedded = new Picture(new ByteVector(ImgData));
                tagfile.Tag.Pictures = new Picture[] { PicEmbedded };
            }
            //Use Internet Picture
            else if (metainfo.albumPic != "")
            {
                byte[]? NetImgData = FetchUrl(new Uri(metainfo.albumPic));
                if (NetImgData is not null)
                {
                    var PicFromNet = new Picture(new ByteVector(NetImgData));
                    tagfile.Tag.Pictures = new Picture[] { PicFromNet };
                }
            }

            //Add more infomation
            tagfile.Tag.Title = metainfo.musicName;
            tagfile.Tag.Performers = metainfo.artist.Select(x => x[0].GetString()).ToArray();
            tagfile.Tag.Album = metainfo.album;
            tagfile.Tag.Subtitle = String.Join(@";", metainfo.alias);
            tagfile.Save();
        }

        private byte[]? FetchUrl(Uri uri)
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

        private uint ReadUint32(ref MemoryStream ms)
        {
            Span<byte> buffer = stackalloc byte[4];
            ms.Read(buffer);
            return MemoryMarshal.Read<uint>(buffer);
        }

        public bool Convert(string path)
        {
            return Task.Run(() => ConvertAsync(path)).Result;
        }

        public async Task<bool> ConvertAsync(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("File Path Not Exist!");
                return false;
            }
            
            //Read all bytes to ram.
            MemoryStream ms =new MemoryStream(await System.IO.File.ReadAllBytesAsync(path));

            //Verify Header
            if (!ReadHeader(ref ms))
            {
                Console.WriteLine("Not a NCM File");
                return false;
            }

            // skip 2 bytes
            ms.Seek(2, SeekOrigin.Current);

            //Make Keybox
            byte[] RC4Key = ReadRC4Key(ref ms);

            //Read Meta Info
            MetaInfo metainfo = ReadMeta(ref ms);
            string format = metainfo.format;

            //CRC32 Check
            var crc32bytes = new byte[4];
            ms.Read(crc32bytes, 0, crc32bytes.Length);
            var crc32len = MemoryMarshal.Read<int>(crc32bytes);

            // skip 5 character,
            ms.Seek(5, SeekOrigin.Current);

            // read image length
            var ImageLength = ReadUint32(ref ms);
            byte[]? ImageData;
            if (ImageLength != 0)
            {
                // read image data
                ImageData = new byte[ImageLength];
                ms.Read(ImageData, 0, ImageData.Length);
            }
            else
            {
                ImageData = null;
            }

            // Read Audio Data
            byte[] AudioData = ReadAudioData(ref ms, RC4Key);

            //Flush Audio Data to disk drive
            string OutputPath = path.Substring(0, path.LastIndexOf("."));

            if (format is null or "") format = "mp3";
            await System.IO.File.WriteAllBytesAsync($"{OutputPath}.{format}", AudioData);

            //Add tag and cover
            AddTag($"{OutputPath}.{format}", ImageData, metainfo);

            return true;
        }
    }
}