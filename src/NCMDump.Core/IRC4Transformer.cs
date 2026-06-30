namespace NCMDump.Core
{
    public interface IRC4Transformer
    {
        public byte[] Transform(byte[] data);

        public int Transform(Memory<byte> data);

        public int Transform(Span<byte> data);
    }
}