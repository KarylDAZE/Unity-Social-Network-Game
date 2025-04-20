using SK.Framework.Sockets;
using proto.Match;
using MySql;
using System.Data;
using proto.CardGame;
using System.Diagnostics;

public static class CardGameMgr
{
    public const int SCORE_UP = 20;
    public const int SCORE_DOWN = 10;
    // 房间类，保存匹配到的两位玩家
    public class Room
    {
        public Client? Player1 { get; set; }
        public Client? Player2 { get; set; }
        public int Player1Hp { get; set; }
        public int Player2Hp { get; set; }
        public int Player1Energy { get; set; }
        public int Player2Energy { get; set; }
        public CardId Player1CardId { get; set; }
        public CardId Player2CardId { get; set; }
    }

    public enum CardId
    {
        attack,
        attack2,
        defend,
        reflect,
        charge
    }
    public enum CardCost
    {
        attack = 1,
        attack2 = 2,
        defend = 1,
        reflect = 1,
        charge = -1
    }
    // 用队列存储等待匹配的玩家
    private static Queue<Client> waitingQueue = new();
    // 存储所有房间的列表
    private static List<Room> rooms = new List<Room>();
    private static Dictionary<int, Room> roomDict = new();
    private static Dictionary<int, int> scoreDict = new();

    public static void ExitCardGame(Client client)
    {
        if (roomDict.ContainsKey(client.userId))
        {
            StopMatch(client);
        }
    }

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
                Player2 = player,
                Player1Hp = 7,
                Player2Hp = 7,
                Player1CardId = (CardId)(-1),
                Player2CardId = (CardId)(-1),
                Player1Energy = 2,
                Player2Energy = 2,
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
                isWin = 1,
                isTurnOver = true,
                selfScore = opponentScore,
            };
            CardGameRes playerRes = new()
            {
                isGameOver = true,
                isWin = -1,
                isTurnOver = true,
                selfScore = playerScore,
            };
            Server.Send(opponent, opponentRes);
            Server.Send(player, playerRes);
        }
    }

    public static void OnCardGameArg(Client sender, CardGameArg arg)
    {
        if (!roomDict.ContainsKey(sender.userId))
        {
            return;
        }
        Room room = roomDict[sender.userId];
        Client opponent = room.Player1 == sender ? room.Player2 : room.Player1;
        // 处理游戏逻辑
        if (room.Player1 == sender)
        {
            room.Player1CardId = (CardId)arg.cardId;
        }
        else
        {
            room.Player2CardId = (CardId)arg.cardId;
        }
        // 检查是否双方都出牌了
        if (room.Player1CardId == (CardId)(-1) || room.Player2CardId == (CardId)(-1))
        {
            return;
        }
        else
        {
            // 双方都出牌了，开始计算结果
            OnTurnOver(room);
        }
    }

    private static void OnTurnOver(Room room)
    {
        int player1Attack = 0;
        int player1Defend = 0;
        int player2Attack = 0;
        int player2Defend = 0;
        bool isGameOver = false;
        int player1IsWin = 0;
        int player2IsWin = 0;
        switch (room.Player1CardId)
        {
            case CardId.attack:
                player1Attack = 1;
                room.Player1Energy -= (int)CardCost.attack;
                break;
            case CardId.attack2:
                player1Attack = 3;
                room.Player1Energy -= (int)CardCost.attack2;
                break;
            case CardId.defend:
                player1Defend = 3;
                room.Player1Energy -= (int)CardCost.defend;
                break;
            case CardId.reflect:
                if (room.Player2CardId == CardId.attack || room.Player2CardId == CardId.attack2)
                {
                    player1Attack = 1;
                }
                player1Defend = 1;
                room.Player1Energy -= (int)CardCost.reflect;
                break;
            case CardId.charge:
                room.Player1Energy -= (int)CardCost.charge;
                break;
        }
        switch (room.Player2CardId)
        {
            case CardId.attack:
                player2Attack = 1;
                room.Player2Energy -= (int)CardCost.attack;
                break;
            case CardId.attack2:
                player2Attack = 3;
                room.Player2Energy -= (int)CardCost.attack2;
                break;
            case CardId.defend:
                player2Defend = 3;
                room.Player2Energy -= (int)CardCost.defend;
                break;
            case CardId.reflect:
                if (room.Player1CardId == CardId.attack || room.Player1CardId == CardId.attack2)
                {
                    player2Attack = 1;
                }
                player2Defend = 1;
                room.Player2Energy -= (int)CardCost.reflect;
                break;
            case CardId.charge:
                room.Player2Energy -= (int)CardCost.charge;
                break;
        }
        if (room.Player1Energy < 0 || room.Player2Energy < 0)
        {
            // 能量不足，客户端数据不合法
            if (room.Player1Energy < 0)
            {
                room.Player1CardId = (CardId)(-1);
            }
            if (room.Player2Energy < 0)
            {
                room.Player2CardId = (CardId)(-1);
            }
            Debug.WriteLine("Player " + room.Player1.userId + " or Player " + room.Player2.userId + " energy is not enough!");
            return;
        }
        room.Player1Hp -= player2Attack - player1Defend > 0 ? player2Attack - player1Defend : 0;
        room.Player2Hp -= player1Attack - player2Defend > 0 ? player1Attack - player2Defend : 0;
        isGameOver = room.Player1Hp <= 0 || room.Player2Hp <= 0;
        if (room.Player1Hp <= 0 && room.Player2Hp <= 0)
        {
            player1IsWin = 0;
            player2IsWin = 0;
        }
        else if (room.Player1Hp <= 0)
        {
            player1IsWin = -1;
            player2IsWin = 1;
            scoreDict[room.Player1.userId] -= SCORE_DOWN;
            scoreDict[room.Player2.userId] += SCORE_UP;
        }
        else if (room.Player2Hp <= 0)
        {
            player1IsWin = 1;
            player2IsWin = -1;
            scoreDict[room.Player1.userId] += SCORE_UP;
            scoreDict[room.Player2.userId] -= SCORE_DOWN;
        }
        CardGameRes player1Res = new()
        {
            isGameOver = isGameOver,
            isWin = player1IsWin,
            isTurnOver = true,
            playerCardId = (int)room.Player2CardId,
            playerHp = room.Player2Hp,
            selfHp = room.Player1Hp,
            selfScore = scoreDict[room.Player1.userId],
            selfEnergy = room.Player1Energy,
        };
        CardGameRes player2Res = new()
        {
            isGameOver = isGameOver,
            isWin = player2IsWin,
            isTurnOver = true,
            playerCardId = (int)room.Player1CardId,
            playerHp = room.Player1Hp,
            selfHp = room.Player2Hp,
            selfScore = scoreDict[room.Player2.userId],
            selfEnergy = room.Player2Energy,
        };
        room.Player1CardId = (CardId)(-1);
        room.Player2CardId = (CardId)(-1);

        if (isGameOver)
        {
            MySqlHelper.UpdateRows("score", new Dictionary<string, object> { { "score", scoreDict[room.Player1.userId] } }, "user_id = " + room.Player1.userId);
            MySqlHelper.UpdateRows("score", new Dictionary<string, object> { { "score", scoreDict[room.Player2.userId] } }, "user_id = " + room.Player2.userId);
            scoreDict.Remove(room.Player1.userId);
            scoreDict.Remove(room.Player2.userId);
            roomDict.Remove(room.Player1.userId);
            roomDict.Remove(room.Player2.userId);
        }

        Server.Send(room.Player1, player1Res);
        Server.Send(room.Player2, player2Res);
    }
}