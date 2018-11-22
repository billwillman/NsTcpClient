using UnityEngine;
using System.Collections;
using NsTcpClient;

public class Test : MonoBehaviour {

    void Start() {
        ProtoMessageMgr.GetInstance().Register<C_S_Login_Req>(C_S_Login_Req.Parser);
        NetManager.Instance.AddPacketListener(100, OnTestProtoCallBack);
    }

    void OnTestProtoCallBack(GamePacket packet) {
        if (packet == null)
            return;
        var rep = packet.ProtoBufToObject<C_S_Login_Req>();
        if (rep != null) {
            var str = rep.ToString();
            Debug.Log(str);
        }
    }


    void OnSocketConnect(bool isConnect)
	{
		Debug.LogFormat("连接状态：{0}", isConnect.ToString());
	}

    void OnSocketAbort()
    {
        Debug.Log("服务器断开连接");
    }

	void OnGUI()
	{
		if (GUI.Button(new Rect(100, 100, 100, 50), "测试网路"))
		{
			Debug.Log("Button Click!!!");

			NetManager.Instance.OnConnectResult = OnSocketConnect;
            NetManager.Instance.OnSocketAbort = OnSocketAbort;
			NetManager.Instance.Disconnect();
			NetManager.Instance.ConnectServer("127.0.0.1", 1024);
            //NetManager.Instance.SendStr("123", 1);
		}
	}

    float lastSendTime = 0;

	void Update()
	{
		TimerMgr.Instance.UnScaleTick(Time.unscaledDeltaTime);
		TimerMgr.Instance.ScaleTick(Time.deltaTime);
        float currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastSendTime >= 3f)
        {
            lastSendTime = currentTime;
            if (NetManager.Instance.ClietnState == eClientState.eClient_STATE_CONNECTED)
            {
                SendProtoMessage();
            }
        }

	}

    void SendProtoMessage()
    {
       // NetManager.Instance.SendMessage(1000, null);
        var req = new C_S_Login_Req();
        req.UserName = "zengyi";
        req.Password = "HelloWorld";
        NetManager.Instance.SendProtoBuf<C_S_Login_Req>(req, 100);
    }

	void OnApplicationQuit()
	{
		NetManager.Instance.Disconnect();
	}

}
