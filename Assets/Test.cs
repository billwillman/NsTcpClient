﻿using UnityEngine;
using System.Collections;
using NsTcpClient;
using CapnProto;
using CapnProto_Msg;
using System.IO;

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

    void TestCapnProto() {
        var msg = ProtoMessageMgr.CreateCapnProtoMsg();
        var loginMsg = LoginMsg.Create(msg.Root);

        /// var userNameMsg = ProtoMessageMgr.CreateCapnProtoMsg();
        loginMsg.userName = ProtoMessageMgr.CreateText(msg, "zengyi");

        // var passWordMsg = ProtoMessageMgr.CreateCapnProtoMsg();
        loginMsg.passWord = ProtoMessageMgr.CreateText(msg, "123");
        loginMsg.userID = 456;

      //  Debug.LogError(loginMsg.userName.ToString());
       // Debug.LogErrorFormat("msg size: {0:D}", userNameMsg.MessageSize);
      
             LoginMsg newLoginMsg;
            ProtoMessageMgr.Parser<LoginMsg>(msg, out newLoginMsg, msg.MessageSize);
        NetManager.Instance.SendCapnProto(msg, 1);
        
        FileStream stream = new FileStream("D:/test.proto", FileMode.Create, FileAccess.Write);
        ProtoMessageMgr.SaveToStream(stream, msg);
        stream.Close();
        stream.Dispose();
        
     //   LoginMsg newMsg;
     //   ProtoMessageMgr.Parser<LoginMsg>(msg.GetBuffer(), out newMsg, msg.MessageSize);

      //'  userNameMsg.Dispose();
       // passWordMsg.Dispose();
        msg.Dispose();
    }

    void TestGamePacket() {
        var packet = GamePacket.CreateFromPool();
        packet.Dispose();
    }

	void Update()
	{
		TimerMgr.Instance.UnScaleTick(Time.unscaledDeltaTime);
		TimerMgr.Instance.ScaleTick(Time.deltaTime);
        float currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastSendTime >= 0f)
        {
            lastSendTime = currentTime;
            if (NetManager.Instance.ClietnState == eClientState.eClient_STATE_CONNECTED)
            {
              //  SendProtoMessage();
            }
        }

        // TestProtoMessage();
         TestCapnProto();
       // TestGamePacket();
    }

    void TestProtoMessage() {
        var req = new C_S_Login_Req();
        req.UserName = "zengyi";
        req.Password = "HelloWorld";
        int dataSize;

        UnityEngine.Profiling.Profiler.BeginSample("ProtoToBuf");
        var stream = ProtoMessageMgr.ToBufferNode<C_S_Login_Req>(req, out dataSize);
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("BufToProto");
        var rep = ProtoMessageMgr.Instance.Parser<C_S_Login_Req>(stream.GetBuffer(), stream.DataSize);
        UnityEngine.Profiling.Profiler.EndSample();

        stream.Dispose();
    }

    void SendProtoMessage()
    {
       // NetManager.Instance.SendMessage(1000, null);
        var req = new C_S_Login_Req();
        req.UserName = "zengyi";
        req.Password = "HelloWorld";
        NetManager.Instance.SendProtoBuf<C_S_Login_Req>(req, 1);
    }

	void OnApplicationQuit()
	{
		NetManager.Instance.Disconnect();
	}

}
