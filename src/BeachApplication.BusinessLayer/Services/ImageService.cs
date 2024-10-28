using AutoMapper;
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

public class ImageService(IApplicationDbContext db, ISqlClientCache cache, IStorageProvider storageProvider, IMapper mapper) : IImageService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var image = await db.GetAsync<Entities.Image>(id);
        if (image is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Image, id));
        }

        await db.DeleteAsync(image);
        await db.SaveAsync();

        await storageProvider.DeleteAsync(image.Path);
        return Result.Ok();
    }

    public async Task<Result<Image>> GetAsync(Guid id)
    {
        var dbImage = await db.GetData<Entities.Image>().FirstOrDefaultAsync(i => i.Id == id);
        if (dbImage is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Image, id));
        }

        var image = mapper.Map<Image>(dbImage);
        return image;
    }

    public async Task<Result<PaginatedList<Image>>> GetListAsync()
    {
        var query = db.GetData<Entities.Image>();
        var totalCount = await query.CountAsync();

        var dbImages = await query.OrderBy(i => i.Path).ToListAsync();
        var images = mapper.Map<IEnumerable<Image>>(dbImages);

        return new PaginatedList<Image>(images, totalCount);
    }

    public async Task<Result<StreamFileContent>> ReadAsync(Guid id)
    {
        var image = await db.GetAsync<Entities.Image>(id);
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

    public async Task<Result<Image>> UploadAsync(Stream stream, string fileName)
    {
        var path = PathGenerator.CreatePath(fileName);
        await storageProvider.SaveAsync(stream, path);

        var image = new Entities.Image
        {
            Path = path,
            Length = stream.Length,
            ContentType = MimeUtility.GetMimeMapping(fileName)
        };

        await db.InsertAsync(image);
        await db.SaveAsync();

        return mapper.Map<Image>(image);
    }
}