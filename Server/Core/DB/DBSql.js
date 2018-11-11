/*
使用的SQL語句
*/

require("../struct/Utils");

SqlObjType = {
    NumberInt: 0,
    String: 1,
    NumberFloat: 2
}

DBSql = {


}

// MongoDB的命令
MongoDBCmd = {
    "1": "{\"user\":\"{0}\", \"password\": \"{1}\"}"

}

DBSql.GetSql =
    function (commandId)
    {
        if (commandId == null)
            return null;
        switch (commandId)
        {
            case 1:
            case 2:
                return MongoDBCmd[1];
        }
        return null;
    }

DBSql.GetCommandKey =
    function (clientId, commandId)
    {
        var key = {"clientId": clientId, "commandId": commandId};
        return key;
    }

DBSql.MongoDBUlr = "mongodb://localhost:27017/data";

DBSql.MongoDB.Tables = 
["user", "player"];

// 插入
DBSql.MongoDB.InsertInTable =
    function (tableName, json, dbServer, key, targetSocket)
    {
        if (tableName == null || json == null || dbServer == null || key == null)
            return false;
        var db = dbServer.GetDB();
        if (db == null)
            return false;
        
        var user = JSON.parse(json);
        var device = db.collection(tableName);
        if (device == null)
            return;
        device.insert(user,
            (err, result)=>
            {
                dbServer.OnDBResult(key, result, error, targetSocket);
            }
            );
        return true;
    }

var setJsonFmt = "{$set: {0}}";
// {$set:{"age":26}}
DBSql.MongoDB.UpdateInTable = 
    function (tableName, whereJson, setJson, dbServer, key, targetSocket, isUpdateAllData)
    {
        if (tableName == null || whereJson == null || 
            setJson == null || dbServer == null || key == null)
            return false;

        var db = dbServer.GetDB();
        if (db == null)
            return false;

        var device = db.collection(tableName);
        if (device == null)
            return false;

        if (isUpdateAllData == null)
          isUpdateAllData = true;
        if (!isUpdateAllData)
            setJson = setJsonFmt.format(setJson);
        device.update(whereJson, setJson, 
            (error, result)=>
            {
                dbServer.OnDBResult(key, result, error, targetSocket);
            }
            );

        return true;
    }

DBSql.MongoDB.FindInTable =
    function (tableName, json, dbServer, key, targetSocket)
    {
        if (tableName == null || json == null || dbServer == null || key == null)
            return false;
        
        var db = dbServer.GetDB();
        if (db == null)
            return false;
        var device = db.collection(tableName);
        if (device == null)
            return false;

        device.find(json,
            (error, result)=>
            {
                dbServer.OnDBResult(key, result, error, targetSocket);
            });
        return true;
    }

// 删除数据
DBSql.MongoDB.DelteInTable =
    function (tableName, whereJson, dbServer, key, targetSocket)
    {
        if (tableName == null || whereJson == null || 
            dbServer == null || key == null)
            return false;

        var db = dbServer.GetDB();
        if (db == null)
            return false;
        var device = db.collection(tableName);
        if (device == null)
            return false;

        device.remove(whereJson,
            (error)=>
            {
                dbServer.OnDBResult(key, null, error, targetSocket);
            });
    }

// 删除表
DBSql.MongoDB.DropTable =
    function (tableName, dbServer, key)
    {
        if (tableName == null || 
            dbServer == null || key == null)
            return false;
        var device = db.collection(tableName);
        if (device == null)
            return false;

        return device.drop();
    }

module.exports = DBSql;