using System.Data;
using BeachApplication.DataAccessLayer.Settings;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BeachApplication.DataAccessLayer;

public class SqlContext(SqlContextOptions options) : ISqlContext
{
    private SqlConnection connection = new SqlConnection(options.ConnectionString);
    private CancellationTokenSource tokenSource = new CancellationTokenSource();

    private bool disposed = false;

    public bool IsOpened
    {
        get
        {
            ThrowIfDisposed();
            return IsOpenedInternal;
        }
    }

    private bool IsOpenedInternal => connection.State == ConnectionState.Open;

    public async Task<IEnumerable<T>> GetDataAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
        where T : class
    {
        ThrowIfDisposed();

        await connection.OpenAsync(tokenSource.Token);
        return await connection.QueryAsync<T>(sql, param, transaction, commandType: commandType);
    }

    public async Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TReturn : class
    {
        ThrowIfDisposed();

        await connection.OpenAsync(tokenSource.Token);
        return await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType);
    }

    public async Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TThrid, TReturn>(string sql, Func<TFirst, TSecond, TThrid, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThrid : class
        where TReturn : class
    {
        ThrowIfDisposed();

        await connection.OpenAsync(tokenSource.Token);
        return await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType);
    }

    public async Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TThrid, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThrid, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThrid : class
        where TFourth : class
        where TReturn : class
    {
        ThrowIfDisposed();

        await connection.OpenAsync(tokenSource.Token);
        return await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType);
    }

    public async Task<T> GetObjectAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
        where T : class
    {
        ThrowIfDisposed();

        await connection.OpenAsync(tokenSource.Token);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandType: commandType);
    }

    public async Task<TReturn> GetObjectAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TReturn : class
    {
        ThrowIfDisposed();

        await connection.OpenAsync(tokenSource.Token);
        return (await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType)).FirstOrDefault();
    }

    public async Task<TReturn> GetObjectAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThird : class
        where TReturn : class
    {
        ThrowIfDisposed();

        await connection.OpenAsync(tokenSource.Token);
        return (await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType)).FirstOrDefault();
    }

    public async Task<TReturn> GetObjectAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThird : class
        where TFourth : class
        where TReturn : class
    {
        ThrowIfDisposed();

        await connection.OpenAsync(tokenSource.Token);
        return (await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType)).FirstOrDefault();
    }

    public async Task<T> GetSingleValueAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
    {
        ThrowIfDisposed();

        await connection.OpenAsync(tokenSource.Token);
        return await connection.ExecuteScalarAsync<T>(sql, param, transaction, commandType: commandType);
    }

    public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
    {
        ThrowIfDisposed();

        await connection.OpenAsync(tokenSource.Token);
        return await connection.ExecuteAsync(sql, param, transaction, commandType: commandType);
    }

    public async Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        ThrowIfDisposed();

        await connection.OpenAsync(tokenSource.Token);
        return await connection.BeginTransactionAsync(isolationLevel);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed && disposing)
        {
            if (connection is not null)
            {
                if (IsOpenedInternal)
                {
                    connection.Close();
                }

                connection.Dispose();
                connection = null;
            }

            if (tokenSource is not null)
            {
                tokenSource.Dispose();
                tokenSource = null;
            }

            disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, GetType().FullName);
    }
}