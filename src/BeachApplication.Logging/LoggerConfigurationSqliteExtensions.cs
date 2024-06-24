﻿using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace BeachApplication.Logging;

public static class LoggerConfigurationSqliteExtensions
{
    public static LoggerConfiguration Sqlite(this LoggerSinkConfiguration sinkConfiguration,
            string databasePath, string tableName, IConfiguration configuration,
            LogEventLevel restrictedtoMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null)
    {
        if (!Uri.TryCreate(databasePath, UriKind.RelativeOrAbsolute, out var pathUri))
        {
            throw new ArgumentException($"Invalid path {nameof(databasePath)}");
        }

        if (!pathUri.IsAbsoluteUri)
        {
            var basePath = AppContext.BaseDirectory;
            databasePath = Path.Combine(Path.GetDirectoryName(basePath) ?? throw new NullReferenceException(), databasePath);
        }

        var sqliteDbFile = new FileInfo(databasePath);
        sqliteDbFile.Directory?.Create();

        var sink = new SqliteBatchSink(sqliteDbFile.FullName, tableName);
        return sinkConfiguration.Sink(sink, restrictedtoMinimumLevel, levelSwitch);
    }
}