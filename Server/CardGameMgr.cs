using SK.Framework.Sockets;
using proto.Match;
using MySql;
using System.Data;
using proto.CardGame;

public static class CardGameMgr
{
    public const int SCORE_UP = 20;
    public const int SCORE_DOWN = 10;
    // 房间类，保存匹配到的两位玩家
    public class Room
    {
        public Client? Player1 { get; set; }
        public Client? Player2 { get; set; }
        // 可以增加房间ID和其他信息
    }

    // 用队列存储等待匹配的玩家
    private static Queue<Client> waitingQueue = new();
    // 存储所有房间的列表
    private static List<Room> rooms = new List<Room>();
    private static Dictionary<int, Room> roomDict = new();
    private static Dictionary<int, int> scoreDict = new();


    public static void OnMatchArg(Client sender, MatchArg arg)
    {
        if (arg.isBeginGame)
        {
            MatchPlayer(sender);
        }
        else
        {
            StopMatch(sender);
        }
    }

    // 处理匹配请求
    public static void MatchPlayer(Client player)
    {
        if (roomDict.ContainsKey(player.userId))
        {
            // 玩家已经在房间中，不能重复匹配
            return;
        }
        if (waitingQueue.Count > 0)
        {
            Client opponent = waitingQueue.Dequeue();
            Room room = new Room
            {
                Player1 = opponent,
                Player2 = player
            };
            rooms.Add(room);
            roomDict.Add(opponent.userId, room);
            roomDict.Add(player.userId, room);
            // 记录玩家分数
            var scoreTable = MySqlHelper.GetTable("score");
            DataRow[] foundRows = scoreTable.Select("user_id = " + opponent.userId);
            int opponentScore = foundRows.Length > 0 ? Convert.ToInt32(foundRows[0]["score"]) : 0;
            scoreDict.Add(opponent.userId, opponentScore);
            foundRows = scoreTable.Select("user_id = " + player.userId);
            int playerScore = foundRows.Length > 0 ? Convert.ToInt32(foundRows[0]["score"]) : 0;
            scoreDict.Add(player.userId, playerScore);

            // TODO: 向两个玩家发送房间信息
            ServerMessageHandler.SendMatchRes(opponent, true, opponentScore, player.username);
            ServerMessageHandler.SendMatchRes(player, true, playerScore, opponent.username);
        }
        else
        {
            waitingQueue.Enqueue(player);
        }
    }

    // 主动退出匹配/认输
    public static void StopMatch(Client player)
    {
        if (waitingQueue.Contains(player))
        {
            waitingQueue = new Queue<Client>(waitingQueue.Where(p => p != player));
            ServerMessageHandler.SendMatchRes(player, false, 0, string.Empty);
        }
        else
        {
            if (!roomDict.ContainsKey(player.userId))
            {
                return;
            }
            Room room = roomDict[player.userId];
            Client opponent = room.Player1 == player ? room.Player2 : room.Player1;
            rooms.Remove(room);
            roomDict.Remove(player.userId);
            roomDict.Remove(opponent.userId);
            // 更新分数
            int opponentScore = scoreDict[opponent.userId] + SCORE_UP;
            int playerScore = scoreDict[player.userId] - SCORE_DOWN;
            MySqlHelper.UpdateRows("score", new Dictionary<string, object> { { "score", opponentScore } }, "user_id = " + opponent.userId);
            MySqlHelper.UpdateRows("score", new Dictionary<string, object> { { "score", playerScore } }, "user_id = " + player.userId);
            scoreDict.Remove(player.userId);
            scoreDict.Remove(opponent.userId);
            CardGameRes opponentRes = new()
            {
                isGameOver = true,
                isWin = true,
                selfScore = opponentScore,
            };
            CardGameRes playerRes = new()
            {
                isGameOver = true,
                isWin = false,
                selfScore = playerScore,
            };
            Server.Send(opponent, opponentRes);
            Server.Send(player, playerRes);
        }
    }
}