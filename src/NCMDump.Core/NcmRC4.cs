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

        public int Transform(Span<byte> data)
        {
            for (int m = 0; m < data.Length; m++)
            {
                i = (byte)(i + 1);
                Si = Sbox[i];
                j = (byte)(i + Si);
                Sj = Sbox[j];

                data[m] ^= Sbox[(byte)(Si + Sj)];
            }
            return data.Length;
        }

        public int Transform(Memory<byte> data)
        {
            return Transform(data.Span);
        }
    }
}