using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using WoWClassic.Common;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Crypto;
using WoWClassic.Common.Log;
using WoWClassic.Common.Network;
using WoWClassic.Common.Packets;

namespace WoWClassic.Gateway
{
    public class GatewayConnection : Connection
    {
        public GatewayConnection(GatewayServer server, Socket socket)
            : base(server, socket)
        {
            GatewaySrv = server;
            HandleAcceptedConnection();
        }

        private static Random s_Rnd = new Random();

        // TODO: Find permanent solution for this
        public readonly GatewayServer GatewaySrv;

        public int Seed { get; set; } = s_Rnd.Next();
        public AuthCrypt Crypt { get; set; }
        public ulong CharacterGUID { get; set; }

        protected override int ProcessInternal(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                var header = new WorldPacketHeader(Crypt, br);
                Log.WriteLine(GatewayLogTypes.Packets, $"<- {header.Opcode}({header.Length}):\n\t{string.Join(" ", data.Select(b => b.ToString("X2")))}");

                try
                {
                    if (!GatewayHandlers.PacketHandlers.ContainsKey(header.Opcode) || !GatewayHandlers.PacketHandlers[header.Opcode](this, br))
                    {
                        if (CharacterGUID == 0)
                            throw new Exception("Packet unhandled by Gateway -- Character GUID = 0");

                        Log.WriteLine(GatewayLogTypes.Packets, $"Forwarding {header.Opcode} to world server");
                        SendWorldPacket(header, data);
                    }
                }
                catch (EndOfStreamException) { return -1; }

                return data.Length; // We used all the data
            }
        }

        public void SendWorldPacket(WorldPacketHeader header, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(CharacterGUID);
                // Replace header with the decrypted one
                Buffer.BlockCopy(header.GetDecrypted(), 0, data, 0, 6);
                bw.Write(data);

                ((GatewayServer)m_Server).SendWorldPacket(this, ms.ToArray());
            }
        }

        public void SendPacket(WorldOpcodes opcode, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(((ushort)(data.Length + 2)).SwitchEndian());
                bw.Write((ushort)opcode);
                bw.Write(data);

                var packet = ms.ToArray();
                Crypt?.Encrypt(packet);
                Log.WriteLine(GatewayLogTypes.Packets, $"-> {opcode}({packet.Length}):\n\t{string.Join(" ", packet.Select(b => b.ToString("X2")))}");
                Send(packet);
            }
        }

        public void SendPacket(byte[] data)
        {
            Crypt?.Encrypt(data);
            Send(data);
        }

        private class SMSG_AUTH_CHALLENGE
        {
            public int Seed;
        }

        private void HandleAcceptedConnection()
        {
            SendPacket(WorldOpcodes.SMSG_AUTH_CHALLENGE, PacketHelper.Build(new SMSG_AUTH_CHALLENGE
            {
                Seed = Seed
            }));
        }


    }
}
