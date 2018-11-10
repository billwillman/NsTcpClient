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
    "1": ""
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

module.exports = DBSql;