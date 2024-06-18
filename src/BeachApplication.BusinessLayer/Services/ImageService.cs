using AutoMapper;
using AutoMapper.QueryableExtensions;
using BeachApplication.BusinessLayer.Internal;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Models;
using BeachApplication.StorageProviders;
using Microsoft.EntityFrameworkCore;
using MimeMapping;
using OperationResults;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class ImageService : IImageService
{
    private readonly IApplicationDbContext applicationDbContext;
    private readonly IStorageProvider storageProvider;
    private readonly IMapper mapper;

    public ImageService(IApplicationDbContext applicationDbContext, IStorageProvider storageProvider, IMapper mapper)
    {
        this.applicationDbContext = applicationDbContext;
        this.storageProvider = storageProvider;
        this.mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var image = await applicationDbContext.GetAsync<Entities.Image>(id);
        if (image is not null)
        {
            await applicationDbContext.DeleteAsync(image);
            await storageProvider.DeleteAsync(image.Path);

            var affectedRows = await applicationDbContext.SaveAsync();
            if (affectedRows > 0)
            {
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ClientError, ErrorMessages.DatabaseDeleteError);
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Image, id));
    }

    public async Task<Result<Image>> GetAsync(Guid id)
    {
        var dbImage = await applicationDbContext.GetAsync<Entities.Image>(id);
        if (dbImage is not null)
        {
            var image = mapper.Map<Image>(dbImage);
            return image;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Image, id));
    }

    public async Task<Result<IEnumerable<Image>>> GetListAsync()
    {
        var query = applicationDbContext.GetData<Entities.Image>();
        var images = await query.ProjectTo<Image>(mapper.ConfigurationProvider).OrderBy(i => i.Path).ToListAsync();

        return images;
    }

    public async Task<Result<StreamFileContent>> ReadAsync(Guid id)
    {
        var image = await applicationDbContext.GetAsync<Entities.Image>(id);
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

    public async Task<Result<Image>> UploadAsync(string fileName, Stream stream, string description, bool overwrite)
    {
        var path = PathGenerator.CreatePath(fileName);
        await storageProvider.SaveAsync(path, stream, overwrite);

        var image = new Entities.Image
        {
            Path = path,
            Length = stream.Length,
            ContentType = MimeUtility.GetMimeMapping(fileName),
            Description = description
        };

        await applicationDbContext.InsertAsync(image);
        var affectedRows = await applicationDbContext.SaveAsync();

        if (affectedRows > 0)
        {
            var savedImage = mapper.Map<Image>(image);
            return savedImage;
        }

        return Result.Fail(FailureReasons.ClientError, ErrorMessages.DatabaseInsertError);
    }
}