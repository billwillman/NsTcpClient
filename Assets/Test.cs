using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NsTcpClient;
using CapnProto;
using CapnProto_Msg;
using CapnpGen;
using System.IO;

public class Test : MonoBehaviour {

    private List<Text> txtList = null;

    void Start() {
        ProtoMessageMgr.GetInstance().Register<C_S_Login_Req>(C_S_Login_Req.Parser);
        NetManager.Instance.AddPacketListener(100, OnTestProtoCallBack);

        txtList = new List<Text>(10);
        
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
        var loginMsg = CapnProto_Msg.LoginMsg.Create(msg.Root);

      //   var userNameMsg = ProtoMessageMgr.CreateCapnProtoMsg();
        loginMsg.userName = ProtoMessageMgr.CreateText(msg, "zengyi");
       // userNameMsg.Dispose();

       //  var passWordMsg = ProtoMessageMgr.CreateCapnProtoMsg();
        loginMsg.passWord = ProtoMessageMgr.CreateText(msg, "123");
      //  passWordMsg.Dispose();

        loginMsg.userID = 456;

       // CapnProtoMsg listMsg = ProtoMessageMgr.CreateCapnProtoMsg();
        txtList.Clear();
        for (int i = 0; i < txtList.Capacity; ++i) {
            txtList.Add(ProtoMessageMgr.CreateText(msg, "abcdef"));
        }
        loginMsg.roleList = ProtoMessageMgr.CreateList<Text>(msg, txtList);

        FileStream stream = new FileStream("D:/test.bin", FileMode.Create);
        MemoryStream memStream = new MemoryStream();
        msg.msg.Write(stream);
        msg.msg.Write(memStream);
        stream.Close();
        stream.Dispose();

        
        Pointer outMsg;
        ProtoMessageMgr.Parser(memStream.GetBuffer(), out outMsg, (int)memStream.Length);
      //  CapnProto.Schema.CodeGeneratorRequest req = (CapnProto.Schema.CodeGeneratorRequest)outMsg;
        memStream.Dispose();
        //  listMsg.Dispose();


        //  Debug.LogError(loginMsg.userName.ToString());
        // Debug.LogErrorFormat("msg size: {0:D}", userNameMsg.MessageSize);

        //      LoginMsg newLoginMsg;
        //     ProtoMessageMgr.Parser<LoginMsg>(msg, out newLoginMsg, msg.MessageSize);
        // NetManager.Instance.SendCapnProto(msg, 1);

        //   LoginMsg newMsg;
        //   ProtoMessageMgr.Parser<LoginMsg>(msg.GetBuffer(), out newMsg, msg.MessageSize);

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
       // TestCanProto_Core();
       // TestGamePacket();
    }

    void TestCanProto_Core() {

        var msg = Capnp.SerializerState.CreateForRpc<CapnpGen.LoginMsg.WRITER>();
        msg.UserName = "zengyi";
        // var list = Capnp.SerializerState.CreateForRpc <Capnp.ListOfTextSerializer>();

        //  list.Init(2);
        //  list[0] = "abcdef";
        ///  list[1] = "abcdef";
        // msg.RoleList = list;
        var list = msg.RoleList;
        list.Init(2);
        list[0] = "abcdef";
        list[1] = "abcdef";
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
