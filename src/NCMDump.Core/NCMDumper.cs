using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TagLib;

namespace NCMDump.Core
{
    public class NCMDumper
    {
        private readonly int vectorSize = Vector256<byte>.Count;
        private static readonly byte[] coreKey = [0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57];
        private static readonly byte[] metaKey = [0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28];

        private bool VerifyHeader(ref MemoryStream ms)
        {
            // Header Should be "CTENFDAM"
            Span<byte> header = stackalloc byte[8];
            ms.Read(header);
            long header_num = MemoryMarshal.Read<long>(header);
            return header_num == 0x4d4144464e455443;
            //return Enumerable.SequenceEqual(header.ToArray(), new byte[] { 0x43, 0x54, 0x45, 0x4E, 0x46, 0x44, 0x41, 0x4D });
        }

        private byte[] ReadRC4Key(ref MemoryStream ms)
        {
            // read keybox length
            uint KeyboxLength = ReadUint32(ref ms);

            //read raw keybox data
            Span<byte> buffer = stackalloc byte[(int)KeyboxLength];
            ref byte ref_buffer = ref MemoryMarshal.GetReference(buffer);
            ms.Read(buffer);

            //SIMD XOR 0x64
            Vector256<byte> xor = Vector256.Create((byte)0x64);
            int i;
            for (i = 0; i <= buffer.Length - vectorSize; i += vectorSize)
            {
                var vb = Vector256.LoadUnsafe(ref ref_buffer, (nuint)i);
                vb ^= xor;
                Vector256.StoreUnsafe(vb, ref ref_buffer, (nuint)i);
            }
            for (; i < buffer.Length; i++)
            {
                buffer[i] ^= 0x64;
            }

            // decrypt keybox data
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.ECB;
                aes.Key = coreKey;
                var cleanText = aes.DecryptEcb(buffer, PaddingMode.PKCS7)[17..];
                return cleanText;
            }
        }

        private MetaInfo ReadMeta(ref MemoryStream ms)
        {
            // read meta length
            var MetaLength = ReadUint32(ref ms);
            Span<byte> buffer = new byte[(int)MetaLength];
            ref byte ref_b = ref MemoryMarshal.GetReference(buffer);
            ms.Read(buffer);

            //SIMD XOR 0x63
            Vector256<byte> xor = Vector256.Create((byte)0x63);
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

            buffer = System.Convert.FromBase64String(Encoding.ASCII.GetString(buffer[22..]));

            // decrypt meta data which is a json contains info of the song
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.ECB;
                aes.Key = metaKey;
                var cleanText = aes.DecryptEcb(buffer, PaddingMode.PKCS7);
                var MetaJsonString = Encoding.UTF8.GetString(cleanText.AsSpan(6));
                JsonSerializerOptions option = new JsonSerializerOptions();
                MetaInfo metainfo = JsonSerializer.Deserialize<MetaInfo>(MetaJsonString)!;
                return metainfo;
            }
        }

        private async Task<byte[]> ReadAudioData(MemoryStream ms, byte[] Key)
        {
            using (IRC4_NCM_Stream rc4s = new RC4_NCM_Stream(ms, Key))
            {
                byte[] data = new byte[ms.Length - ms.Position];
                Memory<byte> m_data = new(data);
                await rc4s.ReadAsync(m_data);
                return data;
            }
        }

        private async Task AddTag(string fileName, byte[]? ImgData, MetaInfo metainfo)
        {
            var tagfile = TagLib.File.Create(fileName);

            //Use Embedded Picture
            if (ImgData is not null)
            {
                var PicEmbedded = new Picture(new ByteVector(ImgData));
                tagfile.Tag.Pictures = [PicEmbedded];
            }
            //Use Internet Picture
            else if (metainfo.albumPic != "")
            {
                byte[]? NetImgData = await FetchUrl(new Uri(metainfo.albumPic));
                if (NetImgData is not null)
                {
                    var PicFromNet = new Picture(new ByteVector(NetImgData));
                    tagfile.Tag.Pictures = [PicFromNet];
                }
            }

            //Add more information
            tagfile.Tag.Title = metainfo.musicName;
            tagfile.Tag.Performers = metainfo.artist.Select(x => x[0]).ToArray();
            tagfile.Tag.Album = metainfo.album;
            tagfile.Tag.Subtitle = string.Join(@";", metainfo.alias);
            tagfile.Save();
        }

        private async Task<byte[]?> FetchUrl(Uri uri)
        {
            HttpClient client = new();
            try
            {
                var response = await client.GetAsync(uri);
                Console.WriteLine(response.StatusCode);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var memStream = new MemoryStream())
                    {
                        response.Content.ReadAsStream().CopyTo(memStream);
                        memStream.Position = 0;
                        Console.WriteLine("album picture Load OK : remote returned {0}", response.StatusCode);
                        return memStream.ToArray();
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
                Console.WriteLine($"File {path} Not Exist!");
                return false;
            }

            //Read all bytes to ram.
            MemoryStream ms = new(await System.IO.File.ReadAllBytesAsync(path));

            //Verify Header
            if (!VerifyHeader(ref ms))
            {
                Console.WriteLine($"{path} is not a NCM File");
                return false;
            }

            // skip 2 bytes
            ms.Seek(2, SeekOrigin.Current);

            //Make Keybox
            byte[] RC4Key = ReadRC4Key(ref ms);

            //Read Meta Info
            MetaInfo metainfo = ReadMeta(ref ms);

            //CRC32 Check
            uint crc32 = ReadUint32(ref ms);

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
            byte[] AudioData = await ReadAudioData(ms, RC4Key);

            //Flush Audio Data to disk drive
            string OutputPath = path[..path.LastIndexOf('.')];

            string format = metainfo.format ?? "mp3";
            await System.IO.File.WriteAllBytesAsync($"{OutputPath}.{format}", AudioData);

            //Add tag and cover
            await Task.Run(() => AddTag($"{OutputPath}.{format}", ImageData, metainfo));
            await ms.DisposeAsync();
            return true;
        }
    }
}