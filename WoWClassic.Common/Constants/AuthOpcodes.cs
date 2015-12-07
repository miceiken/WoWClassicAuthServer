namespace WoWClassic.Common.Constants
{
    public enum AuthOpcodes : byte
    {
        AuthLogonChallenge = 0x00,
        AuthLogonProof = 0x01,
        AuthReconnectChallenge = 0x02,
        AuthReconnectProof = 0x03,
        RealmList = 0x10,
    };
}
