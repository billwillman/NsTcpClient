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

class DBServer extends NetManager
{
    constructor(port)
    {
       super();
       this.m_CommandList = new LinkedList();
       this.m_CommandMap = new Dictionary();
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

    // 執行隊列
    Run()
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

        process.nextTick(this.Run);
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
        // 开始运行
        server.Run();
        return server;
  }

module.exports = DBServer;