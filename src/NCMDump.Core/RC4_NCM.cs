namespace NCMDump.Core
{
    public class RC4_NCM
    {
        private readonly byte[] Keybox;
        private int i = 0, j = 0;

        public RC4_NCM(byte[] key)
        {
            Keybox = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();

            //Generate Keybox
            for (int x = 0, y = 0; x < 256; x++)
            {
                y = (y + Keybox[x] + key[x % key.Length]) & 0xFF;
                (Keybox[x], Keybox[y]) = (Keybox[y], Keybox[x]);
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            Encrypt(data.AsSpan());
            return data;
        }

        public int Encrypt(Span<byte> data)
        {
            for (int m = 0; m < data.Length; m++)
            {
                i = (i + 1) & 0xFF;
                j = (i + Keybox[i]) & 0xFF;
                data[m] ^= Keybox[(Keybox[i] + Keybox[j]) & 0xFF];
            }
            return data.Length;
        }

        public int Encrypt(Memory<byte> data)
        {
            return Encrypt(data.Span);
        }

        public byte[] Decrypt(byte[] data)
        {
            return Encrypt(data);
        }

        public int Decrypt(Span<byte> data)
        {
            return Encrypt(data);
        }

        public int Decrypt(Memory<byte> data)
        {
            return Encrypt(data);
        }
    }
}