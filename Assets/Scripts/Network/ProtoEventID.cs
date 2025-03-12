namespace Multiplayer
{
    /// <summary>
    /// 各协议类型事件ID
    /// </summary>
    public static class ProtoEventID
    {
        public static readonly int AvatarProperty = typeof(proto.AvatarProperty.AvatarProperty).GetHashCode();
        public static readonly int LoginArg = typeof(proto.Login.LoginArg).GetHashCode();
        public static readonly int LoginRes = typeof(proto.Login.LoginRes).GetHashCode();
    }
}