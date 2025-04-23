using System.Data;
using System.Diagnostics;
using MySql;
using proto.CardGame;
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
        private const int MAX_MESSAGE_COUNT = 50;
        private static Dictionary<string, bool> loginedUsers = new Dictionary<string, bool>();

        public static void OnLoginArg(Client sender, IExtensible proto)
        {
            var res = new proto.Login.LoginRes
            {
                ErrCode = (int)ErrCode.SUCCESS
            };
            var arg = proto as proto.Login.LoginArg;
            if (null == arg)
            {
                Debug.WriteLine("Invalid proto type");
                return;
            }

            // 检查是否已经登录
            if (loginedUsers.ContainsKey(arg.username))
            {
                res.ErrCode = (int)ErrCode.COMMON_FAIL;
                Server.Send(sender, res);
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
                // 插入新的一行到score表
                int userId;
                dataTable = MySqlHelper.GetTable("user_info");
                foundRows = dataTable.Select("username = '" + arg.username + "'");
                userId = Convert.ToInt32(foundRows[0]["id"]);
                MySqlHelper.InsertRow("score", new Dictionary<string, object>()
                    {
                        { "user_id", userId },
                        { "score", 0 }
                    });
            }
            if (res.ErrCode == (int)ErrCode.SUCCESS)
            {
                // 登录成功
                dataTable = MySqlHelper.GetTable("user_info");
                foundRows = dataTable.Select("username = '" + arg.username + "'");
                res.id = sender.userId = Convert.ToInt32(foundRows[0]["id"]);
                res.username = sender.username = arg.username;
                loginedUsers.Add(arg.username, true);
            }
            Server.Send(sender, res);
        }

        public static void Logout(Client sender)
        {
            if (loginedUsers.ContainsKey(sender.username))
            {
                loginedUsers.Remove(sender.username);
            }
        }

        public static void OnFriendInfoArg(Client sender)
        {
            FriendMgr.OnFriendInfoArg(sender);
        }

        public static void OnFriendOperationArg(Client sender, IExtensible proto)
        {
            var arg = proto as proto.FriendOperation.FriendOperationArg;
            if (null == arg)
            {
                Debug.WriteLine("Invalid proto type");
                return;
            }
            FriendMgr.OnFriendOperationArg(sender, arg);
        }

        public static void OnMessageArg(Client sender, IExtensible proto)
        {
            var arg = proto as proto.Message.MessageArg;
            if (null == arg)
            {
                Debug.WriteLine("Invalid proto type");
                return;
            }
            FriendMgr.OnMessageArg(sender, arg);
        }

        public static void OnMatchArg(Client sender, IExtensible proto)
        {
            var arg = proto as proto.Match.MatchArg;
            if (null == arg)
            {
                Debug.WriteLine("Invalid proto type");
                return;
            }
            CardGameMgr.OnMatchArg(sender, arg);
        }

        public static void SendMatchRes(Client sender, bool isBeginGame, int score, string playerUsername)
        {
            var res = new proto.Match.MatchRes
            {
                isBeginGame = isBeginGame,
                Score = score,
                PlayerUsername = playerUsername
            };
            Server.Send(sender, res);
        }

        public static void SendMatchRes(Client sender, bool isBeginGame, int score, string playerUsername, int selfHp, int selfEnergy)
        {
            var res = new proto.Match.MatchRes
            {
                isBeginGame = isBeginGame,
                Score = score,
                PlayerUsername = playerUsername,
                selfHp = selfHp,
                selfEnergy = selfEnergy
            };
            Server.Send(sender, res);
        }

        public static void OnCardGameArg(Client sender, IExtensible proto)
        {
            var arg = proto as CardGameArg;
            if (null == arg)
            {
                Debug.WriteLine("Invalid proto type");
                return;
            }
            CardGameMgr.OnCardGameArg(sender, arg);
        }

        public static void SendCardGameRes(Client sender, CardGameRes cardGameRes)
        {
            Server.Send(sender, cardGameRes);
        }
    }
}