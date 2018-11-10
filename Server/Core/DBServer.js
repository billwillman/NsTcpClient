/*
  DB服务器
*/

var NetManager = require("./NetManager");
var DefaultPacketHandler = require("./DefaultPacketHandler");
var RegisterDBMessage = require("./RegisterDBMessage");
var LinkedList = require("./struct/LinkedList");
var Dictionary = require("./struct/Dictionary");
var DBCommand = require("./DB/DBCommand");
require("./struct/Utils");
var MongoClient = require('mongodb').MongoClient
    , assert = require('assert');

class DBServer extends NetManager
{
    constructor(port)
    {
       super();
       this.m_CommandList = new LinkedList();
       this.m_CommandMap = new Dictionary();
       this.m_IsConnectDB = false;
       this.m_DB = null;
       this.m_IsRunQueue = false;
    }

    GetDB()
    {
      return this.m_DB;
    }

    // 推入參數隊列
    Push(key, sql, paramArgs)
    {
       var command = this.m_CommandMap.GetValue(key);
       if (command == null)
       {
          command = new DBCommand(key, sql, paramArgs);
          var linkedNode = this.m_CommandList.AddLastValue(command);
          command.AttachLinkedNode(linkedNode);
          this.m_CommandMap.Add(key, command);
       } else
       {
          command.ChangeParamArgs(paramArgs);
       }
    }

    Run()
    {
      if (this.m_IsRunQueue)
        return;
      this.m_IsRunQueue = true;
      this._Run();
    }

    // 執行隊列
    _Run()
    {
        if (this.m_CommandList != null)
        {
            var first = this.m_CommandList.GetFirstNode();
            
            if (first != null)
            {
              this.m_CommandList.RemoveFirstNode();
              var command = first.GetValue();
              RunCommand(command);
              var key = command.GetKey();
              this.m_CommandMap.RemoveKey(key);
            }
        }

        process.nextTick(this._Run);
    }

    // 執行命令
    RunCommand(command)
    {
      if (command == null)
        return;
       var fmt = command.m_sql;
       var args = command.m_ParamArgs;
       var sql = fmt.format(args);
       SendToDBSql(sql);
    }

    // 發送給DB數據庫SQL
    SendToDBSql(sql)
    {
      if (sql == null)
        return;
      
    }


    CloseDB()
    {
      this.m_IsConnectDB = false;
      try
      {
        if (this.m_DB != null)
        {
          this.m_DB.close();
          this.m_DB = null;
        }
      } catch
      {}
    }

    // 连接DB
    Start()
    {
      this.CloseDB();

      MongoClient.connect(DBSql.MongoDBUlr, (err, db)=>
      {
        if (err != null)
        {
           // 出错了
           this.m_IsConnectDB = false;
           this.KickGSAndLS();
           // 重连数据库
           this.Start();
           return;
        }

        this.m_DB = db;
        this.m_IsConnectDB = this.m_DB != null;
        
        // MongoDB不需要查看TABLE是否存在，会自动创建表结构

        // 开启队列执行
        this.Run();
      }
      );
    }

    /* 继承接口 */
    OnConnectedEvent(clientSocket)
    {
      // 没有连接数据库
       if (!this.m_IsConnectDB)
       {
          if (clientSocket != null)
          {
            clientSocket.destroy();
            clientSocket = null;
          }
          return;
       }
       super.OnConnectedEvent(clientSocket);
    }

    KickGSAndLS()
    {
       this.CloseAllClientSocket();
    }

    // 关闭
    Destroy()
    {
      this.KickGSAndLS();
      this.CloseDB();
    }


    // 从DB返回
    OnDBResult(key, result, error)
    {

    }
}

// 创建DB服务器
DBServer.Create =
  function (port)
  {
        if (port == null)
          return null;
        var server = new DBServer(port);
        server.SetPacketHandlerClass(DefaultPacketHandler);

        new RegisterDBMessage();
         // 5秒钟必须要有数据接收, 心跳包
        server.Listen(port, 5000);
        // 连接DB
        server.Start();
        return server;
  }

module.exports = DBServer;