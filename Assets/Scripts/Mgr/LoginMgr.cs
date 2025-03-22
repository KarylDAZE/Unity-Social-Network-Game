using UnityEngine;
using SK.Framework;
using SK.Framework.UI;
using Multiplayer;
using ProtoBuf;

namespace Mgr
{
    public class LoginMgr : MonoBehaviour
    {
        private static LoginMgr instance;
        public static LoginMgr Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = FindObjectOfType<LoginMgr>();
                }
                return instance;
            }
        }

        public string Username;

        public int Id;


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

        void Start()
        {
            if (-2 == Main.UI.LoadView("LoginWindow", UIConst.LoginWindow, ViewLevel.NORMAL, out _, null, true))
            {
                Debug.LogError("Load LoginWindow failed");
                return;
            }

            // network connect
            Main.Custom.Network.Connect("127.0.0.1", 8801);
        }

        public void SendLogin(string username, string password)
        {
            var loginArg = new proto.Login.LoginArg
            {
                username = username,
                //password encryption
                password = password
            };
            Main.Custom.Network.Send(loginArg);
        }

        public void OnLogin(IExtensible proto)
        {
            var res = proto as proto.Login.LoginRes;
            Id = res.id;
            Username = res.username;
            Main.Events.Publish(ProtoEventID.LoginRes, 0 == res.ErrCode);
        }
    }
}