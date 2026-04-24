using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TagLib;

namespace NCMDump.Core
{
    public sealed class NCMDumper
    {
        private static readonly int vectorSize = Vector256<byte>.Count;
        private static readonly byte[] coreKey = [0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57];
        private static readonly byte[] metaKey = [0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28];

        private const int MaxStackallocSize = 4096; // 4 KB safety limit for stackalloc

        private bool VerifyHeader(MemoryStream ms)
        {
            // Header Should be "CTENFDAM"
            Span<byte> header = stackalloc byte[8];
            int bytesRead = ms.Read(header);
            if (bytesRead < 8)
                return false;
            return header.SequenceEqual("CTENFDAM"u8);
        }

        private byte[] ReadRC4Key(MemoryStream ms)
        {
            // read keybox length
            uint keyboxLength = ReadUint32(ms);

            if (keyboxLength > MaxStackallocSize)
                throw new InvalidDataException($"Keybox data too large: {keyboxLength} bytes (max {MaxStackallocSize}).");

            Span<byte> buffer = stackalloc byte[(int)keyboxLength];

            ref byte refBuffer = ref MemoryMarshal.GetReference(buffer);
            int bytesRead = ms.Read(buffer);
            if (bytesRead < (int)keyboxLength)
                throw new InvalidDataException("Incomplete keybox data in NCM file.");

            // SIMD XOR 0x64
            Vector256<byte> xor = Vector256.Create((byte)0x64);
            int i;
            for (i = 0; i <= buffer.Length - vectorSize; i += vectorSize)
            {
                var vb = Vector256.LoadUnsafe(ref refBuffer, (nuint)i);
                vb ^= xor;
                Vector256.StoreUnsafe(vb, ref refBuffer, (nuint)i);
            }
            for (; i < buffer.Length; i++)
            {
                buffer[i] ^= 0x64;
            }

            // decrypt keybox data
            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Key = coreKey;
            var cleanText = aes.DecryptEcb(buffer, PaddingMode.PKCS7)[17..];
            return cleanText;
        }

        private MetaInfo ReadMeta(MemoryStream ms)
        {
            // read meta length
            var metaLength = ReadUint32(ms);
            Span<byte> buffer = new byte[(int)metaLength];
            ref byte refBuffer = ref MemoryMarshal.GetReference(buffer);
            int bytesRead = ms.Read(buffer);
            if (bytesRead < (int)metaLength)
                throw new InvalidDataException("Incomplete metadata in NCM file.");

            // SIMD XOR 0x63
            Vector256<byte> xor = Vector256.Create((byte)0x63);
            int i;
            for (i = 0; i <= buffer.Length - vectorSize; i += vectorSize)
            {
                var vb = Vector256.LoadUnsafe(ref refBuffer, (nuint)i);
                vb ^= xor;
                Vector256.StoreUnsafe(vb, ref refBuffer, (nuint)i);
            }
            for (; i < buffer.Length; i++)
            {
                buffer[i] ^= 0x63;
            }

            buffer = Convert.FromBase64String(Encoding.ASCII.GetString(buffer[22..]));

            // decrypt meta data which is a json contains info of the song
            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Key = metaKey;
            var cleanText = aes.DecryptEcb(buffer, PaddingMode.PKCS7);
            var metaJsonString = Encoding.UTF8.GetString(cleanText.AsSpan(6));

            MetaInfo metainfo = JsonSerializer.Deserialize(metaJsonString, MetaInfoJsonContext.Default.MetaInfo)!;
            return metainfo;
        }

        private async Task<byte[]> ReadAudioData(MemoryStream ms, byte[] key, CancellationToken cancellationToken = default)
        {
            using NcmRC4Stream rc4s = new(ms, key);
            byte[] audioData = new byte[ms.Length - ms.Position];
            Memory<byte> audioDataView = new(audioData);
            await rc4s.ReadExactlyAsync(audioDataView, cancellationToken);
            return audioData;
        }

        private async Task AddTag(string fileName, byte[]? imgData, MetaInfo metainfo, CancellationToken cancellationToken = default)
        {
            try
            {
                var tagfile = TagLib.File.Create(fileName);

                // Use Embedded Picture
                if (imgData is not null)
                {
                    var picEmbedded = new Picture([.. imgData]);
                    tagfile.Tag.Pictures = [picEmbedded];
                }
                // Use Internet Picture
                else if (!string.IsNullOrEmpty(metainfo.AlbumPic))
                {
                    byte[]? netImgData = await FetchUrl(new Uri(metainfo.AlbumPic), cancellationToken);
                    if (netImgData is not null)
                    {
                        var picFromNet = new Picture([.. netImgData]);
                        tagfile.Tag.Pictures = [picFromNet];
                    }
                }

                // Add more information
                tagfile.Tag.Title = metainfo.MusicName;
                tagfile.Tag.Performers = metainfo.Artist?.Select(x => x?[0]).Where(x => x is not null).ToArray() ?? [];
                tagfile.Tag.Album = metainfo.Album;
                tagfile.Tag.Subtitle = string.Join(";", metainfo.Alias ?? []);
                tagfile.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Warning: Failed to write tags for {0}: {1}", fileName, ex.Message);
            }
        }

        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        private async Task<byte[]?> FetchUrl(Uri uri, CancellationToken cancellationToken = default)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                var response = await _httpClient.GetAsync(uri, cts.Token);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsByteArrayAsync(cancellationToken);
                }

                Console.WriteLine("Failed to download album picture: remote returned {0}", response.StatusCode);
                return null;
            }
            catch (HttpRequestException e)
            {
                System.Diagnostics.Debug.WriteLine("FetchUrl error: {0}", e.Message);
                return null;
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("FetchUrl timed out: {0}", uri);
                return null;
            }
        }

        private uint ReadUint32(MemoryStream ms)
        {
            Span<byte> buffer = stackalloc byte[4];
            int bytesRead = ms.Read(buffer);
            if (bytesRead < 4)
                throw new InvalidDataException("Insufficient data to read uint32.");
            return MemoryMarshal.Read<uint>(buffer);
        }

        /// <summary>
        /// Converts an NCM file to playable audio format (MP3/FLAC) asynchronously.
        /// </summary>
        /// <param name="path">Path to the .ncm file.</param>
        /// <param name="outputDir">Optional output directory. If null, output is placed next to the source file.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        public async Task<bool> ConvertAsync(string path, string? outputDir = null, CancellationToken cancellationToken = default)
        {
            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine($"File {path} Not Exist!");
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Read all bytes to ram.
            using MemoryStream ms = new(await System.IO.File.ReadAllBytesAsync(path, cancellationToken));

            // Verify Header
            if (!VerifyHeader(ms))
            {
                Console.WriteLine($"{path} is not a NCM File");
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // skip 2 bytes
            ms.Seek(2, SeekOrigin.Current);

            // Make Keybox
            byte[] rc4Key = ReadRC4Key(ms);

            // Read Meta Info
            MetaInfo metainfo = ReadMeta(ms);

            // CRC32 Check (value is read but not verified; the NCM format spec stores it here)
            ReadUint32(ms);

            cancellationToken.ThrowIfCancellationRequested();

            // skip 5 character
            ms.Seek(5, SeekOrigin.Current);

            // read image length
            var imageLength = ReadUint32(ms);
            byte[]? imageData;
            if (imageLength != 0)
            {
                // read image data
                imageData = new byte[imageLength];
                int bytesRead = ms.Read(imageData, 0, imageData.Length);
                if (bytesRead < (int)imageLength)
                    throw new InvalidDataException("Incomplete image data in NCM file.");
            }
            else
            {
                imageData = null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Read Audio Data
            byte[] audioData = await ReadAudioData(ms, rc4Key, cancellationToken);

            // Write Audio Data to disk
            string outputFileName = Path.ChangeExtension(Path.GetFileName(path), string.IsNullOrEmpty(metainfo.Format) ? ".mp3" : $".{metainfo.Format}");
            string outputPath = outputDir is not null
                ? Path.Combine(outputDir, outputFileName)
                : Path.Combine(Path.GetDirectoryName(path)!, outputFileName);
            await System.IO.File.WriteAllBytesAsync(outputPath, audioData, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // Add tag and cover
            await AddTag(outputPath, imageData, metainfo, cancellationToken);
            return true;
        }
    }
}
