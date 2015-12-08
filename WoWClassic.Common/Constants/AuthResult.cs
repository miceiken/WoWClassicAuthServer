namespace WoWClassic.Common.Constants
{
    public enum AuthResult : byte
    {
        Success = 0x00,
        Banned = 0x03,
        UnknownAccount,
        IncorrectPassword,
        InvalidVersion = 0x09,
        Suspended = 0x0C,
    };
}
