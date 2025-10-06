namespace NCMDump.Core
{
    public class NcmRC4
    {
        private readonly byte[] Sbox;
        private byte i = 0, j = 0, Si = 0, Sj = 0;

        public NcmRC4(byte[] key)
        {
            Sbox = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();

            //Generate Keybox
            for (int x = 0, y = 0; x < 256; x++)
            {
                y = (y + Sbox[x] + key[x % key.Length]) & 0xFF;
                (Sbox[x], Sbox[y]) = (Sbox[y], Sbox[x]);
            }
        }

        public byte[] Transform(byte[] data)
        {
            Transform(data.AsSpan());
            return data;
        }

        public unsafe int Transform(Span<byte> data)
        {
            fixed (byte* dataPtr = data)
            {
                fixed (byte* sboxPtr = Sbox)
                {
                    int len = data.Length;

                    for (int m = 0; m < len; m++)
                    {
                        byte Si = sboxPtr[(byte)(m + 1)];
                        byte Sj = sboxPtr[(byte)(m + 1 + Si)];
                        dataPtr[m] ^= sboxPtr[(byte)(Si + Sj)];
                    }
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