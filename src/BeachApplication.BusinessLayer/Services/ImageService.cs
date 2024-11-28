using AutoMapper;
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

public class ImageService(IDataContext dataContext, IStorageProvider storageProvider, IMapper mapper) : IImageService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var image = await dataContext.GetAsync<Entities.Image>(id);
        if (image is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Image, id));
        }

        await dataContext.DeleteAsync(image);
        await dataContext.SaveAsync();

        await storageProvider.DeleteAsync(image.Path);
        return Result.Ok();
    }

    public async Task<Result<Image>> GetAsync(Guid id)
    {
        var dbImage = await dataContext.GetData<Entities.Image>().FirstOrDefaultAsync(i => i.Id == id);
        if (dbImage is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Image, id));
        }

        var image = mapper.Map<Image>(dbImage);
        return image;
    }

    public async Task<Result<PaginatedList<Image>>> GetListAsync()
    {
        var query = dataContext.GetData<Entities.Image>();
        var totalCount = await query.CountAsync();

        var dbImages = await query.OrderBy(i => i.Path).ToListAsync();
        var images = mapper.Map<IEnumerable<Image>>(dbImages);

        return new PaginatedList<Image>(images, totalCount);
    }

    public async Task<Result<StreamFileContent>> ReadAsync(Guid id)
    {
        var image = await dataContext.GetAsync<Entities.Image>(id);
        if (image is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, EntityNames.Image, id));
        }

        var stream = await storageProvider.ReadAsStreamAsync(image.Path);
        if (stream is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No image found");
        }

        return new StreamFileContent(stream, image.ContentType);
    }

    public async Task<Result<Image>> UploadAsync(Stream stream, string fileName)
    {
        var path = PathGenerator.CreatePath(fileName);
        await storageProvider.SaveAsync(stream, path);

        var dbImage = new Entities.Image
        {
            Path = path,
            Length = stream.Length,
            ContentType = MimeUtility.GetMimeMapping(fileName)
        };

        await dataContext.InsertAsync(dbImage);
        await dataContext.SaveAsync();

        var image = mapper.Map<Image>(dbImage);
        return image;
    }
}