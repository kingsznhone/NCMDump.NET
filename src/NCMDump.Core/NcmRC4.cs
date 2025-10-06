namespace NCMDump.Core
{
    public sealed class NcmRC4: IRC4Transformer
    {
        private readonly byte[] Sbox;
        private byte i = 0, j = 0, Si = 0, Sj = 0;

        public unsafe NcmRC4(byte[] key)
        {
            Sbox = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                Sbox[i] = (byte)i;
            }
            fixed (byte* sboxPtr = Sbox)
            {
                //Generate Keybox
                for (int x = 0, y = 0; x < 256; x++)
                {
                    y = (y + sboxPtr[x] + key[x % key.Length]) & 0xFF;
                    (sboxPtr[x], sboxPtr[y]) = (sboxPtr[y], sboxPtr[x]);
                }
            }
        }

        public byte[] Transform(byte[] data)
        {
            Transform(data.AsSpan());
            return data;
        }

        public unsafe int Transform(Span<byte> data)
        {
            fixed (byte* sboxPtr = Sbox)
            {
                int len = data.Length;

                for (int m = 0; m < len; m++)
                {
                    byte Si = sboxPtr[(m + 1)&0xFF];
                    byte Sj = sboxPtr[(m + 1 + Si) & 0xFF];
                    data[m] ^= sboxPtr[(Si + Sj) & 0xFF];
                }
            }
            return data.Length;
        }

        public int Transform(Memory<byte> data)
        {
            return Transform(data.Span);
        }
    }
}