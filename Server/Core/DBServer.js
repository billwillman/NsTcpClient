/*
  DB服务器
*/

var NetManager = require("./NetManager");
var DefaultPacketHandler = require("./DefaultPacketHandler");
var RegisterDBMessage = require("./RegisterDBMessage");
var LinkedList = require("./struct/LinkedList");
var Dictionary = require("./struct/Dictionary");
var DBCommand = require("./DB/DBCommand");
var MessageConsts = require("./MessageConsts");
var S_C_LoginRep = require("./Messages/Server/S_C_LoginRep");
require("./struct/Utils");
require("./DB/DBSql");
var MongoClient = require('mongodb').MongoClient
    , assert = require('assert');

class DBServer extends NetManager
{
    constructor(port)
    {
       super();
       // 请求列表
       this.m_CommandList = new LinkedList();
       this.m_CommandMap = new Dictionary();
       //---------------

       this.m_IsConnectDB = false;
       this.m_DB = null;
       this.m_IsRunQueue = false;

       this.m_ClientArr = [0];
    }

    GetDB()
    {
      return this.m_DB;
    }

    // 推入參數隊列
    Push(key, sql, paramArgs, targetSocket)
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
              var key = command.GetKey();
              this.m_CommandMap.RemoveKey(key);

              this.RunCommand(command);
            }
        }

        process.nextTick(
          ()=>
          {
            this._Run();
          }

        );
    }

    // 从DB返回
    OnDBResult(key, result, error, targetSocket)
    {
      if (error != null)
      {
        this.SendToGSError(key, error);
        return;
      }

      var args = [key.clientId];
      // 发送给GS， GS转发给客户端
      var commandId = key.commandId;
      switch (commandId)
      {
        // 注册用户
        case 1:
          break;
        // 检测登录用户
        case 2:
          this.SendMessage(MessageConsts.FromDBMessage.S_User_LoginRet, 
            new S_C_LoginRep(S_C_LoginRet.Sucess), args, targetSocket);
          break;
      }
    }

    // 執行命令
    RunCommand(command)
    {
      if (command == null)
        return;
       var fmt = command.m_sql;
       var args = command.m_ParamArgs;
       var sql = fmt.format(args);
       var key = command.GetKey();
       var targetSocket = command.GetTargetSocket();
       SendToDBSql(key, sql, targetSocket);
    }

    // 發送給DB數據庫SQL
    SendToDBSql(key, sql, targetSocket)
    {
      if (key == null || sql == null)
        return;
       var commandId = key.commandId;
       var ret = false;
       switch (commandId)
       {
         // 注册用户
          case 1:
            ret = DBSql.MongoDB.InsertInTable(DBSql.MongoDB.Tables[0], sql, this, key, targetSocket);
            break;
          // 查询用户
          case 2:
            ret = DBSql.MongoDB.FindInTable(DBSql.MongoDB.Tables[0], sql, this, key, targetSocket);
            break;
       }

       if (!ret)
       {
          this.SendToGSError(key, null, targetSocket)
       }
    }

    SendToGSError(key, error, targetSocket)
    {
       var commandId = key.commandId;
       var clientId = key.clientId;


       this.m_ClientArr[0] = clientId;
       // 发送失败
       switch (commandId)
       {
          case 1:
            break;
          case 2:
            {
              if (error == null)
              {
                 this.SendMessage(MessageConsts.FromDBMessage.S_User_LoginRet, 
                  new S_C_LoginRep(S_C_LoginRet.DBServerError), this.m_ClientArr, targetSocket);
              } else
              {
                this.SendMessage(MessageConsts.FromDBMessage.S_User_LoginRet, 
                  new S_C_LoginRep(S_C_LoginRet.DBSqlError), this.m_ClientArr, targetSocket);
              }
              break;
            }
       }
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

      MongoClient.connect(DBSql.MongoDBUlr, {useNewUrlParser: true}, (err, db)=>
      {
        if (err != null)
        {
           // 出错了
           this.m_IsConnectDB = false;
           this.KickGSAndLS();
           // 重连数据库
           process.nextTick(()=>
           {
              this.Start();
           });
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