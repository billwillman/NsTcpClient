using UnityEngine;
using System.Collections;
using NsTcpClient;

public class Test : MonoBehaviour {

	void OnSocketConnect(bool isConnect)
	{
		Debug.LogFormat("连接状态：{0}", isConnect.ToString());
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(100, 100, 100, 50), "测试网路"))
		{
			Debug.Log("Button Click!!!");

			NetManager.Instance.OnConnectResult = OnSocketConnect;
			NetManager.Instance.Disconnect();
			NetManager.Instance.ConnectServer("www.baidu.com", 80);
			NetManager.Instance.SendStr("你好，百度", 1);
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
