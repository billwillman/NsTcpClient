using UnityEngine;
using System.Collections;
using NsTcpClient;
using CapnProto;
using CapnProto_Msg;

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
        var node = NetByteArrayPool.GetByteBufferNode();
        Message mm = Message.Load(node.GetBuffer(), 0, node.Length);
        var req = mm.Root;
        var loginMsg = LoginMsg.Create(req);
        loginMsg.IsValid();
        node.Dispose();
        mm.Dispose();
        return;

        UnityEngine.Profiling.Profiler.BeginSample("CapnProto Create");
        var msg = ProtoMessageMgr.CreateCapnProtoMsg<LoginMsg>();
        UnityEngine.Profiling.Profiler.EndSample();

        int destSize = msg.data.ByteLength();
        var destNode = NetByteArrayPool.GetByteBufferNode(destSize);
        System.Array.Copy(msg.allocator.Buffer, destNode.Buffer, destSize);

        UnityEngine.Profiling.Profiler.BeginSample("CapnProto Serial");
        LoginMsg destMsg;
        if (ProtoMessageMgr.Parser<LoginMsg>(destNode.Buffer, out destMsg, destNode.DataSize)) {
        }
        destNode.Dispose();
        UnityEngine.Profiling.Profiler.EndSample();


        UnityEngine.Profiling.Profiler.BeginSample("CapnProto Dispose");
        msg.Dispose();
        UnityEngine.Profiling.Profiler.EndSample();
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
