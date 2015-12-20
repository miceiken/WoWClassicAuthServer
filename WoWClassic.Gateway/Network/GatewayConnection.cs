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

        public string AccountName;
        public int AccountID;

        protected override int ProcessInternal(byte[] data)
        {
            var packets = WorldPacket.FromBuffer(data, flags: WorldPacketFlags.EncryptedHeader | WorldPacketFlags.BigEndianLength, crypt: Crypt);
            var bytesRead = 0;
            foreach (var pkt in packets)
            {
                Log.WriteLine(GatewayLogTypes.Packets, $"<- {pkt.Header.Opcode}({pkt.Header.Length}):\n\t{string.Join(" ", pkt.Payload.Select(b => b.ToString("X2")))}");

                var buffer = pkt.Payload.ToArray();
                using (var ms = new MemoryStream(buffer))
                using (var br = new BinaryReader(ms))
                {
                    if (!GatewayHandlers.PacketHandlers.ContainsKey(pkt.Header.Opcode) || !GatewayHandlers.PacketHandlers[pkt.Header.Opcode](this, br))
                    {
                        if (CharacterGUID == 0)
                            throw new Exception("Packet unhandled by Gateway -- Character GUID = 0");

                        Log.WriteLine(GatewayLogTypes.Packets, $"Forwarding {pkt.Header.Opcode} to world server");
                        SendWorldPacket(pkt.Header, buffer);
                    }
                }

                bytesRead += pkt.TotalLength;
            }

            return bytesRead;
        }

        public void SendWorldPacket(WorldPacketHeader header, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(CharacterGUID);
                bw.Write(header.GetDecrypted());
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
            // Swap header length endianness
            Array.Reverse(data, 0, 2);
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
