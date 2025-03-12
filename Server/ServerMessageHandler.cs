using System.Data;
using System.Diagnostics;
using MySql;
using ProtoBuf;

namespace SK.Framework.Sockets
{
    public enum ErrCode
    {
        SUCCESS = 0,
        COMMON_FAIL = -1
    }
    public static class ServerMessageHandler
    {
        public static void OnAvatarProperty(Client sender, IExtensible proto)
        {
            Server.Send(proto, sender);
        }
        public static void OnLoginArg(Client sender, IExtensible proto)
        {
            var res = new proto.Login.LoginRes
            {
                ErrCode = (int)ErrCode.SUCCESS
            };
            // database check
            var arg = proto as proto.Login.LoginArg;
            if (null == arg)
            {
                Debug.WriteLine("Invalid proto type");
                return;
            }

            DataTable dataTable = MySqlHelper.GetTable("user_info");
            // 查找username等于arg.username的行
            DataRow[] foundRows = dataTable.Select("username = '" + arg.username + "'");
            if (foundRows.Length > 0)
            {
                // 找到账号，比较password是否一致
                if (foundRows[0]["password"].ToString() != arg.password)
                {
                    // 密码不匹配，处理错误，比如设置一个错误标记或错误消息
                    res.ErrCode = (int)ErrCode.COMMON_FAIL;
                }
            }
            else
            {
                // 未找到账号，则插入新的一行
                MySqlHelper.InsertRow("user_info", new Dictionary<string, object>()
                {
                    { "username", arg.username },
                    { "password", arg.password },
                    { "avatar_base64", "" }
                });
            }
            Server.Send(sender, res);
        }
    }
}