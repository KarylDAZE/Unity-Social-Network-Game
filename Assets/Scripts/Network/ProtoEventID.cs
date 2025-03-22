namespace Multiplayer
{
    /// <summary>
    /// 各协议类型事件ID
    /// </summary>
    public static class ProtoEventID
    {
        public static readonly int LoginRes = typeof(proto.Login.LoginRes).GetHashCode();
        public static readonly int FriendInfoRes = typeof(proto.FriendInfo.FriendInfoRes).GetHashCode();
        public static readonly int FriendOperationRes = typeof(proto.FriendOperation.FriendOperationRes).GetHashCode();
        public static readonly int MessageRes = typeof(proto.Message.MessageRes).GetHashCode();
    }
}