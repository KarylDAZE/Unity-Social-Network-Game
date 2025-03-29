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
            var res = new proto.FriendInfo.FriendInfoRes
            {
                ErrCode = (int)ErrCode.SUCCESS
            };

            DataTable dataTable = MySqlHelper.GetTable("friend");
            DataRow[] foundRows = dataTable.Select("user_id1 = " + sender.userId + " and status = 1 or user_id2 = " + sender.userId);
            if (foundRows.Length > 0)
            {
                foreach (DataRow row in foundRows)
                {
                    int friendId = row["user_id1"].ToString() == sender.userId.ToString() ? Convert.ToInt32(row["user_id2"]) : Convert.ToInt32(row["user_id1"]);
                    DataRow friendRow = MySqlHelper.GetRow("user_info", friendId);
                    if (null == friendRow)
                    {
                        continue;
                    }
                    res.FriendList.Add(new proto.FriendInfo.PlayerInfo
                    {
                        id = friendId,
                        username = friendRow["username"].ToString(),
                        isFriend = "1" == row["status"].ToString()
                    });
                }
            }

            Server.Send(sender, res);
        }

        public static void OnFriendOperationArg(Client sender, IExtensible proto)
        {
            var arg = proto as proto.FriendOperation.FriendOperationArg;
            if (null == arg)
            {
                Debug.WriteLine("Invalid proto type");
                return;
            }
            if (arg.id == sender.userId)
            {
                return;
            }

            DataTable dataTable = MySqlHelper.GetTable("friend");
            if (arg.isAdd)
            {
                DataRow[] foundRows = dataTable.Select("user_id1 = " + sender.userId + " and user_id2 = " + arg.id + " or user_id1 = " + arg.id + " and user_id2 = " + sender.userId + " and status = 1"),
                foundRows1 = dataTable.Select("user_id1 = " + arg.id + " and user_id2 = " + sender.userId + " and status = 0");
                if (foundRows.Length > 0)
                {
                    // 不需要再次添加
                    return;
                }
                else if (foundRows1.Length > 0)
                {
                    MySqlHelper.UpdateRows("friend", new Dictionary<string, object>()
                    {
                        { "status", 1 }
                    }, "user_id1 = " + arg.id + " and user_id2 = " + sender.userId);
                }
                else
                {
                    // 添加好友请求
                    MySqlHelper.InsertRow("friend", new Dictionary<string, object>()
                    {
                        { "user_id1", sender.userId },
                        { "user_id2", arg.id },
                        { "status", 0 }
                    });
                }
            }
            else
            {
                MySqlHelper.DeleteRows("friend", "user_id1 = " + sender.userId + " and user_id2 = " + arg.id + " or user_id1 = " + arg.id + " and user_id2 = " + sender.userId);
            }
            OnFriendInfoArg(sender);
        }

        public static void OnMessageArg(Client sender, IExtensible proto)
        {
            var arg = proto as proto.Message.MessageArg;
            if (null == arg)
            {
                Debug.WriteLine("Invalid proto type");
                return;
            }

            if (arg.text != string.Empty)
                MySqlHelper.InsertRow("chat", new Dictionary<string, object>()
            {
                { "user_id1", sender.userId },
                { "user_id2", arg.FriendId },
                { "content", arg.text }
            });

            SendMessageRes(sender, arg.FriendId);
        }

        public static void SendMessageRes(Client sender, int friendId)
        {
            var res = new proto.Message.MessageRes
            {
                FriendId = friendId
            };
            DataTable dataTable = MySqlHelper.GetTable("chat");
            DataRow[] foundRows;
            if (0 == friendId)
                // 返回世界消息
                foundRows = dataTable.Select("user_id2 = 0");
            else
                foundRows = dataTable.Select("user_id1 = " + sender.userId + " and user_id2 = " + friendId + " or user_id1 = " + friendId + " and user_id2 = " + sender.userId);
            if (foundRows.Length > 0)
            {
                foreach (DataRow row in foundRows)
                {
                    DataRow playerRow = MySqlHelper.GetRow("user_info", Convert.ToInt32(row["user_id1"]));
                    if (null == playerRow)
                    {
                        continue;
                    }
                    res.messages.Add(new proto.Message.UserMessage
                    {
                        text = row["content"].ToString(),
                        playerInfo = new proto.Message.PlayerInfo
                        {
                            id = Convert.ToInt32(row["user_id1"]),
                            username = playerRow["username"].ToString(),
                        }
                    });
                }
            }
            if (0 == friendId)
                Server.Send(res);
            else
            {
                Server.Send(sender, res);
                foreach (Client client in Server.clients.Values)
                {
                    if (client.userId == friendId)
                    {
                        Server.Send(client, res);
                        break;
                    }
                }
            }
        }

        public static void OnMatchArg(Client sender, IExtensible proto)
        {
            var arg = proto as proto.Match.MatchArg;
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

        public static void OnCardGameArg(Client sender, IExtensible proto)
        {
            var arg = proto as proto.CardGame.CardGameArg;
            if (null == arg)
            {
                Debug.WriteLine("Invalid proto type");
                return;
            }
            // CardGameMgr.OnCardGameArg(sender, arg);
        }

        public static void SendCardGameRes(Client sender, CardGameRes cardGameRes)
        {
            Server.Send(sender, cardGameRes);
        }
    }
}