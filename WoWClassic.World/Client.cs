using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Packets;
using System.IO;
using WoWClassic.Common.Log;
using WoWClassic.Common;

namespace WoWClassic.World
{
    public class Client
    {
        public Client(ulong characterGUID)
        {
            GUID = characterGUID;
        }

        public ulong GUID { get; private set; }

        public void SendPacket(WorldOpcodes opcode, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(GUID);
                bw.Write(((ushort)(data.Length + 2)).SwitchEndian());
                bw.Write((ushort)opcode);
                bw.Write(data);

                var packet = ms.ToArray();
                Log.WriteLine(WorldLogTypes.Packets, $"-> {opcode}({packet.Length}):\n\t{string.Join(" ", packet.Select(b => b.ToString("X2")))}");
                WorldManager.Instance.Gateway.SendPacket(packet);
            }
        }

    }
}
