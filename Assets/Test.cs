using UnityEngine;
using System.Collections;
using NsTcpClient;

public class Test : MonoBehaviour {

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
            NetManager.Instance.SendStr("123", 1);
		}
	}

	void Update()
	{
		TimerMgr.Instance.UnScaleTick(Time.unscaledDeltaTime);
		TimerMgr.Instance.ScaleTick(Time.deltaTime);
	}

	void OnApplicationQuit()
	{
		NetManager.Instance.Disconnect();
	}

}
