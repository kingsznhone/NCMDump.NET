using System.Text;

namespace NCMDumpCore
{
    /// <summary>
    /// In Cloud Music. There is a modified RC4 encryptor.
    /// Not standard RC4 algorithm.
    /// </summary>
    public class RC4_NCM
    {
        public byte[] Keybox;
        private int i = 0, j = 0;

        public RC4_NCM(byte[] key)
        {
            Keybox = new byte[256];

            for (int k = 0; k < 256; k++)
            {
                Keybox[k] = (byte)k;
            }

            //Generate Keybox
            j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + Keybox[i] + key[i % key.Length]) & 0xFF;
                (Keybox[i], Keybox[j]) = (Keybox[j], Keybox[i]);
            }
            i = j = 0;
        }

        public byte[] Encrypt(byte[] data)
        {
            byte[] encrypted = new byte[data.Length];
            for (int m = 0; m < data.Length; m++)
            {
                i = (i + 1) & 0xFF;
                j = (i + Keybox[i]) & 0xFF;
                encrypted[m] = (byte)(data[m] ^ Keybox[(Keybox[i] + Keybox[j]) & 0xFF]);
            }
            return encrypted;
        }

        public int Encrypt(Span<byte> data)
        {
            for (int m = 0; m < data.Length; m++)
            {
                i = (i + 1) & 0xFF;
                j = (i + Keybox[i]) & 0xFF;
                data[m] = (byte)(data[m] ^ Keybox[(Keybox[i] + Keybox[j]) & 0xFF]);
            }
            return data.Length;
        }

        public byte[] Decrypt(byte[] data)
        {
            return Encrypt(data);
        }

        public int Decrypt(Span<byte> data)
        {
            return Encrypt(data);
        }
    }

}