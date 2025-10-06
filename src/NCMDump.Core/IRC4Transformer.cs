using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCMDump.Core
{
    public interface IRC4Transformer
    {
        public byte[] Transform(byte[] data);

        public int Transform(Memory<byte> data);

        public int Transform(Span<byte> data);


    }
}
