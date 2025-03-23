using UnityEngine;
using SK.Framework;
using SK.Framework.UI;
using Multiplayer;
using ProtoBuf;
using System.IO;
using System.Text;

namespace Mgr
{
    public class UserInfo
    {
        public string Username;
        public string Password;
        public bool IsRemember;
    }

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

        #region local

        private string GetMd5String(string str)
        {
            byte[] byteStr = Encoding.UTF8.GetBytes(str);
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] md5Byte = md5.ComputeHash(byteStr);
            return System.BitConverter.ToString(md5Byte).Replace("-", "").ToLower();
        }

        public void SetUserInfo(string username, string password, bool isRemember)
        {
            //save to local
            UserInfo info = new UserInfo
            {
                Username = username,
                Password = isRemember ? password : string.Empty,
                IsRemember = isRemember
            };
            string json = JsonUtility.ToJson(info);
            string filePath = Path.Combine(Application.persistentDataPath, "userInfo.json");
            File.WriteAllText(filePath, json);
        }

        public UserInfo GetUserInfo()
        {
            string filePath = Path.Combine(Application.persistentDataPath, "userInfo.json");
            if (!File.Exists(filePath))
            {
                return null;
            }
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<UserInfo>(json);
        }

        #endregion

        #region proto

        public void SendLogin(string username, string password)
        {
            var loginArg = new proto.Login.LoginArg
            {
                username = username,
                password = GetMd5String(password)
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

    #endregion
}