﻿namespace BeachApplication.StorageProviders;

public interface IStorageProvider : IDisposable
{
    Task DeleteAsync(string path);

    Task<Stream?> ReadAsStreamAsync(string path);

    Task SaveAsync(string path, Stream stream, bool overwrite = false);
}