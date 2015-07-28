using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WoWClassicServer.AuthServer.Constants
{
    public unsafe struct AuthLogonChallenge
    {
        public byte Error;                      // 0x0
        public ushort Size;                     // 0x1
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] GameName;                 // 0x3
        public byte Version1;                   // 0x7
        public byte Version2;                   // 0x8
        public byte Version3;                   // 0x9
        public ushort Build;                    // 0xA
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Platform;                   // 0xC
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] OS;                       // 0x10
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Country;                  // 0x14
        public uint TimezoneBias;               // 0x18
        public uint IP;                         // 0x1C
        public byte I_len;                      // 0x20

        public string Account;

        public static AuthLogonChallenge Read(BinaryReader br)
        {
            var ch = new AuthLogonChallenge();

            ch.Error = br.ReadByte();
            ch.Size = BitConverter.ToUInt16(br.ReadBytes(2), 0);
            ch.GameName = new byte[4];
            for (int i = 3; i >= 0; i--)
                ch.GameName[i] = br.ReadByte();
            ch.Version1 = br.ReadByte();
            ch.Version2 = br.ReadByte();
            ch.Version3 = br.ReadByte();
            ch.Build = BitConverter.ToUInt16(br.ReadBytes(2), 0);
            ch.Platform = new byte[4];
            for (int i = 3; i >= 0; i--)
                ch.Platform[i] = br.ReadByte();
            ch.OS = new byte[4];
            for (int i = 3; i >= 0; i--)
                ch.OS[i] = br.ReadByte();
            ch.Country = new byte[4];
            for (int i = 3; i >= 0; i--)
                ch.Country[i] = br.ReadByte();
            ch.TimezoneBias = BitConverter.ToUInt32(br.ReadBytes(4), 0);
            ch.IP = BitConverter.ToUInt32(br.ReadBytes(4), 0);
            ch.I_len = br.ReadByte();

            ch.Account = new string(br.ReadChars(ch.I_len));

            return ch;
        }

        public override string ToString()
        {
            return "Error=" + Error + ", Size=" + Size + ", GameName=" + Encoding.ASCII.GetString(GameName) + ", Version1=" + Version1 + ", Version2=" + Version2
                + ", Version3=" + Version3 + ", Build=" + Build + ", Platform=" + Encoding.ASCII.GetString(Platform) + ", OS=" + Encoding.ASCII.GetString(OS)
                + ", Country=" + Encoding.ASCII.GetString(Country) + " TimeZone=" + TimezoneBias + ", IP=" + IP
                + ", AcctLen=" + I_len + ", Acct=" + Account;
        }
    }
}
