namespace WoWClassicServer.AuthServer.Constants
{
    public enum AuthCommand : byte
    {
        AuthLogonChallenge = 0x00,
        AuthLogonProof = 0x01,
        AuthReconnectChallenge = 0x02,
        AuthReconnectProof = 0x03,
        RealmList = 0x10,
        CMD_XFER_INITIATE = 0x30,
        CMD_XFER_DATA = 0x31,
        // these opcodes no longer exist in currently supported client
        CMD_XFER_ACCEPT = 0x32,
        CMD_XFER_RESUME = 0x33,
        CMD_XFER_CANCEL = 0x34
    };
}
