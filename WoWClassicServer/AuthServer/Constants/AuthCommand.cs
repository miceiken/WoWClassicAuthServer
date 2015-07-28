namespace WoWClassicServer.AuthServer.Constants
{
    public enum AuthCommand : byte
    {
        CMD_AUTH_LOGON_CHALLENGE = 0x00,
        CMD_AUTH_LOGON_PROOF = 0x01,
        CMD_AUTH_RECONNECT_CHALLENGE = 0x02,
        CMD_AUTH_RECONNECT_PROOF = 0x03,
        CMD_REALM_LIST = 0x10,
        CMD_XFER_INITIATE = 0x30,
        CMD_XFER_DATA = 0x31,
        // these opcodes no longer exist in currently supported client
        CMD_XFER_ACCEPT = 0x32,
        CMD_XFER_RESUME = 0x33,
        CMD_XFER_CANCEL = 0x34
    };
}
