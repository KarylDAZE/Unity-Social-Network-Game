using UnityEngine;
using SK.Framework;
using SK.Framework.UI;

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

        private string username;
        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
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

        // Update is called once per frame
        void Update()
        {

        }
    }
}