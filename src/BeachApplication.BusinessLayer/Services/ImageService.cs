﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using BeachApplication.BusinessLayer.Internal;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Caching;
using BeachApplication.Shared.Models;
using BeachApplication.StorageProviders;
using Microsoft.EntityFrameworkCore;
using MimeMapping;
using OperationResults;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class ImageService(IApplicationDbContext context, ISqlClientCache cache, IStorageProvider storageProvider, IMapper mapper) : IImageService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var image = await context.GetAsync<Entities.Image>(id);
        if (image is not null)
        {
            await context.DeleteAsync(image);
            await storageProvider.DeleteAsync(image.Path);

            await context.SaveAsync();
            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Image, id));
    }

    public async Task<Result<Image>> GetAsync(Guid id)
    {
        var dbImage = await context.GetAsync<Entities.Image>(id);
        if (dbImage is not null)
        {
            var image = mapper.Map<Image>(dbImage);
            return image;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Image, id));
    }

    public async Task<Result<IEnumerable<Image>>> GetListAsync()
    {
        var images = await context.GetData<Entities.Image>()
            .OrderBy(i => i.Path).ProjectTo<Image>(mapper.ConfigurationProvider)
            .ToListAsync();

        return images;
    }

    public async Task<Result<StreamFileContent>> ReadAsync(Guid id)
    {
        var image = await context.GetAsync<Entities.Image>(id);
        if (image is not null)
        {
            var stream = await storageProvider.ReadAsStreamAsync(image.Path);
            if (stream is not null)
            {
                return new StreamFileContent(stream, image.ContentType);
            }
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Image, id));
    }

    public async Task<Result<Image>> UploadAsync(string fileName, Stream stream)
    {
        var path = PathGenerator.CreatePath(fileName);
        await storageProvider.SaveAsync(path, stream, true);

        var image = new Entities.Image
        {
            Path = path,
            Length = stream.Length,
            ContentType = MimeUtility.GetMimeMapping(fileName)
        };

        await context.InsertAsync(image);
        await context.SaveAsync();

        await cache.SetAsync(image, TimeSpan.FromHours(1));
        return mapper.Map<Image>(image);
    }
}