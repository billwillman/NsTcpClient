using UnityEngine;
using System.Collections;
using NsTcpClient;
using Utils;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        NetManager.Instance.OnConnectResult = OnSocketConnected;
        NetManager.Instance.OnSocketAbort = OnSocketAbort;
	}

    void OnSocketConnected(bool isOk)
    {
        Debug.LogFormat ("Socket Connect result: {0}", isOk.ToString ());
    }

    void OnSocketAbort()
    {
        Debug.Log ("Socket abort");
    }
	
	// Update is called once per frame
	void Update () {
        TimerMgr.Instance.ScaleTick (Time.deltaTime);
        TimerMgr.Instance.UnScaleTick (Time.unscaledDeltaTime);
	}

    void OnGUI()
    {
        if (GUI.Button (new Rect(100, 100, 100, 50), "Connect")) {
            NetManager.Instance.Disconnect ();
            NetManager.Instance.ConnectServer ("www.baidu.com", 80);
        }
    }
}
