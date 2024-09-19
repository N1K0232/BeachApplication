using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BeachApplication.DataAccessLayer;

public partial class ApplicationDbContext : ISqlContext
{
    private SqlConnection connection;
    private bool disposed;

    private SqlConnection Connection
    {
        get
        {
            ThrowIfDisposed();
            connection ??= new SqlConnection(Database.GetConnectionString());

            return connection;
        }
    }

    public async Task<IEnumerable<T>> GetDataAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
        where T : class
    {
        await Connection.OpenAsync();
        return await Connection.QueryAsync<T>(sql, param, transaction, commandType: commandType);
    }

    public async Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TReturn : class
    {
        await Connection.OpenAsync();
        return await Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType);
    }

    public async Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TThrid, TReturn>(string sql, Func<TFirst, TSecond, TThrid, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThrid : class
        where TReturn : class
    {
        await Connection.OpenAsync();
        return await Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType);
    }

    public async Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TThrid, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThrid, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThrid : class
        where TFourth : class
        where TReturn : class
    {
        await Connection.OpenAsync();
        return await Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType);
    }

    public async Task<T> GetObjectAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
        where T : class
    {
        await Connection.OpenAsync();
        return await Connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandType: commandType);
    }

    public async Task<TReturn> GetObjectAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TReturn : class
    {
        await Connection.OpenAsync();
        return (await Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType)).FirstOrDefault();
    }

    public async Task<TReturn> GetObjectAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThird : class
        where TReturn : class
    {
        await Connection.OpenAsync();
        return (await Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType)).FirstOrDefault();
    }

    public async Task<TReturn> GetObjectAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThird : class
        where TFourth : class
        where TReturn : class
    {
        await Connection.OpenAsync();
        return (await Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType)).FirstOrDefault();
    }

    public async Task<T> GetSingleValueAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
    {
        await Connection.OpenAsync();
        return await Connection.ExecuteScalarAsync<T>(sql, param, transaction, commandType: commandType);
    }

    public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
    {
        await Connection.OpenAsync();
        return await Connection.ExecuteAsync(sql, param, transaction, commandType: commandType);
    }

    public async Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        await Connection.OpenAsync();
        return await Connection.BeginTransactionAsync(isolationLevel);
    }

    private async ValueTask DisposeConnectionAsync()
    {
        if (connection is not null)
        {
            if (connection.State is ConnectionState.Open)
            {
                await connection.CloseAsync();
            }

            await connection.DisposeAsync();
            connection = null;
        }
    }
}