using System.Data;
using Microsoft.Data.Sqlite;
using Serilog.Core;
using Serilog.Events;

namespace BeachApplication.Logging;

public class SqliteBatchSink(string databasePath, string tableName) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        CreateTable();

        using var connection = CreateConnection();
        var commandText = $"INSERT INTO {tableName} (Message, Level, Timestamp, Exception) " +
               $"VALUES (@message, @level, @timestamp, @exception)";

        using var command = new SqliteCommand(commandText, connection);

        CreateParameter(command, "@message", DbType.String, logEvent.RenderMessage());
        CreateParameter(command, "@level", DbType.String, logEvent.Level.ToString());
        CreateParameter(command, "@timestamp", DbType.DateTime, logEvent.Timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        CreateParameter(command, "@exception", DbType.String, logEvent.Exception?.ToString());

        command.ExecuteNonQuery();
    }

    private void CreateTable()
    {
        using var connection = CreateConnection();
        var columnDefitions = "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
               "Message TEXT, " +
               "Level VARCHAR(10), " +
               "Timestamp TEXT, " +
               "Exception TEXT";

        var commandText = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnDefitions})";

        using var sqlCommand = new SqliteCommand(commandText, connection);
        sqlCommand.ExecuteNonQuery();
    }

    private SqliteConnection CreateConnection()
    {
        var builder = new SqliteConnectionStringBuilder { DataSource = databasePath };
        var connection = new SqliteConnection(builder.ConnectionString);

        connection.Open();
        return connection;
    }

    private static void CreateParameter(IDbCommand command, string name, DbType type, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Value = value ?? DBNull.Value;

        command.Parameters.Add(parameter);
    }
}