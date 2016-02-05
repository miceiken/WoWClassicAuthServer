using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.DataStructure;
using System.IO;
using System.IO.Compression;

namespace WoWClassic.World
{
    public enum ObjectUpdateFlags : byte
    {
        None = 0x0000,
        Self = 0x0001,
        Transport = 0x0002,
        FullGUID = 0x0004,
        HighGUID = 0x0008,
        All = 0x0010,
        Living = 0x0020,
        HasPosition = 0x0040
    };

    public class ObjectUpdate
    {
        public ObjectUpdate()
        { }

        public ObjectUpdate(bool hasTransport)
        {
            HasTransport = hasTransport;
        }

        public bool HasTransport { get; set; } = false;
        //public List<WoWGUID> OutOfRange { get; private set; } = new List<WoWGUID>();
        public List<WoWGUID> InRange { get; private set; } = new List<WoWGUID>();
        public List<byte[]> Blocks { get; private set; } = new List<byte[]>();

        public byte[] Build()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                //bw.Write((uint)(OutOfRange.Count == 0 ? Blocks.Count : Blocks.Count + 1));
                bw.Write((uint)Blocks.Count);
                bw.Write((byte)(HasTransport ? 1 : 0));
                // TODO: this 
                //if (OutOfRange.Count > 0)
                //{ }

                // TODO: Could probably optimize this eventually

                foreach (var block in Blocks)
                    bw.Write(block);

                return ms.ToArray();
            }
        }

        public byte[] Compress(byte[] data)
        {
            using (var uncompressed = new MemoryStream(data))
            using (var compressed = new MemoryStream())
            using (var ds = new DeflateStream(compressed, CompressionMode.Compress))
            {
                uncompressed.CopyTo(ds);

                return compressed.ToArray();
            }
        }
    }
}
