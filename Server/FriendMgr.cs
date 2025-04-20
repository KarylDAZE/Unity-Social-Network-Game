using SK.Framework.Sockets;
using MySql;
using System.Data;
using proto.FriendOperation;
using proto.Message;

public static class FriendMgr
{
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

    public static void OnFriendOperationArg(Client sender, FriendOperationArg arg)
    {
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

    public static void OnMessageArg(Client sender, MessageArg arg)
    {
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
}