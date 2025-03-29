using System.Collections.Generic;
using UnityEngine;
using SK.Framework;
using ProtoBuf;
using proto.FriendOperation;
using proto.FriendInfo;
using proto.Message;
using Multiplayer;


namespace Mgr
{
    public class FriendMgr : MonoBehaviour
    {
        private static FriendMgr instance;
        public static FriendMgr Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = FindObjectOfType<FriendMgr>();
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

        public int PlayerInfoButton = "PlayerInfoButton".GetHashCode();

        public List<proto.FriendInfo.PlayerInfo> FriendList { get; private set; } = new List<proto.FriendInfo.PlayerInfo>();

        public bool GetIsFriend(int id)
        {
            foreach (var friend in FriendList)
            {
                if (friend.id == id)
                {
                    return true;
                }
            }
            return false;
        }

        public void SendFriendInfo()
        {
            var info = new FriendInfoArg();
            Main.Custom.Network.Send(info);
        }

        public void OnFriendInfo(IExtensible proto)
        {
            var res = proto as FriendInfoRes;
            if (res.ErrCode != 0)
            {
                Debug.LogError("Get friend list failed");
                return;
            }

            FriendList = res.FriendList;
            Main.Events.Publish(ProtoEventID.FriendInfoRes);
        }

        public void SendFriendOperation(int id, bool isAdd)
        {
            var info = new FriendOperationArg
            {
                id = id,
                isAdd = isAdd
            };
            Main.Custom.Network.Send(info);
        }

        public void OnFriendOperation(IExtensible proto)
        {
            var res = proto as FriendOperationRes;
            if (res.ErrCode != 0)
            {
                Debug.LogError("Friend operation failed");
                return;
            }
        }

        public void SendMessage(string text, int id)
        {
            var info = new MessageArg
            {
                text = text,
                FriendId = id
            };
            Main.Custom.Network.Send(info);
        }

        public void OnMessage(IExtensible proto)
        {
            var res = proto as MessageRes;
            Main.Events.Publish(ProtoEventID.MessageRes, res);
        }
    }
}