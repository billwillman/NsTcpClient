/*
使用的SQL語句
*/

SqlObjType = {
    NumberInt: 0,
    String: 1,
    NumberFloat: 2
}

DBSql = {


}

// MongoDB的命令
MongoDBCmd = {
    // Inser User in userTable
    "1": "{\"user\":\"{0}\", \"password\": \"{1}\"}",
    // find User by user
    "2":"{\"user\":\"{0}\"}"
}

DBSql.GetSql =
    function (commandId)
    {
        if (commandId == null)
            return null;
        return null;
    }

DBSql.GetCommandKey =
    function (clientId, commandId)
    {
        var key = {"clientId": clientId, "commandId": commandId};
        return key;
    }

DBSql.MongoDBUlr = "mongodb://localhost:27017/data";

// 插入账号数据
DBSql.MongoDB.InsertUserTable =
    function (userJson, dbServer, key)
    {
        if (userJson == null || dbServer == null || key == null)
            return;
        var db = dbServer.GetDB();
        if (db == null)
            return;
        
        var user = JSON.parse(userJson);
        var device = db.collection("user");
        if (device == null)
            return;
        device.insert(user,
            (err, result)=>
            {
                dbServer.OnDBResult(key, result, error);
            }
            );
    }

// 从USER表里通过名字查询账号信息
DBSql.MongoDB.FindUserInTable =
    function (userNameJson, dbServer, key)
    {
        if (userNameJson == null || dbServer == null || key == null)
            return;
        
        var db = dbServer.GetDB();
        if (db == null)
            return;
        var device = db.collection("user");
        if (device == null)
            return;

        device.find(userNameJson,
            (error, result)=>
            {
                dbServer.OnDBResult(key, result, error);
            });
        
    }

module.exports = DBSql;