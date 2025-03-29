using UnityEngine;
using SK.Framework;
using ProtoBuf;
using proto.Match;
using proto.CardGame;
using Multiplayer;

namespace Mgr
{
    public class CardGameMgr : MonoBehaviour
    {
        private static CardGameMgr instance;
        public static CardGameMgr Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = FindObjectOfType<CardGameMgr>();
                }
                return instance;
            }
        }

        void Awake()
        {
            if (null == instance)
            {
                instance = this;
            }
            else if (this != instance)
            {
                Destroy(gameObject);
            }
        }

        #region proto

        public void SendMatch(bool isBeginGame)
        {
            var info = new MatchArg()
            {
                isBeginGame = isBeginGame,
            };
            Main.Custom.Network.Send(info);
        }

        public void OnMatch(IExtensible proto)
        {
            var res = proto as MatchRes;
            Main.Events.Publish(ProtoEventID.MatchRes, res);
        }

        public void SendCardGame()
        {

        }

        public void OnCardGame(IExtensible proto)
        {
            var res = proto as CardGameRes;
            Main.Events.Publish(ProtoEventID.CardGameRes, res);
        }

        #endregion
    }
}