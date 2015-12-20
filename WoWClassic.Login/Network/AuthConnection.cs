using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using WoWClassic.Common;
using WoWClassic.Common.Protocol;
using WoWClassic.Common.Packets;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Log;
using WoWClassic.Common.Network;
using WoWClassic.Common.Crypto;

namespace WoWClassic.Login
{
    public class AuthConnection : Connection
    {
        public AuthConnection(Server server, Socket socket)
            : base(server, socket)
        { }

        protected override int ProcessInternal(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                var command = (AuthOpcodes)br.ReadByte();
                try
                {
                    if (!AuthHandler.PacketHandlers.ContainsKey(command) || !AuthHandler.PacketHandlers[command](this, br))
                        return 0; // TODO: Fix
                }
                catch (EndOfStreamException) { return -1; }

                return data.Length;
            }
        }

        public void SendPacket(AuthOpcodes opcode, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)opcode);
                bw.Write(data);

                var packet = ms.ToArray();
                Log.WriteLine(AuthLogTypes.Packets, $"-> {opcode}({packet.Length}):\n\t{string.Join(" ", packet.Select(b => b.ToString("X2")))}");
                Send(packet);
            }
        }

        public AuthHandler.C_AuthLogonChallenge LogonChallenge { get; set; }
        public AuthHandler.C_AuthLogonProof LogonProof { get; set; }
        public SRP SRP { get; set; }
        public byte[] ChallengeData { get; set; }

        public bool IsAuthenticated
        {
            get
            {
                return SRP != null && SRP.ClientProof == SRP.GenerateClientProof();
            }
        }
    }
}
