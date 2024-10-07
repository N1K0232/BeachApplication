using AutoMapper;
using AutoMapper.QueryableExtensions;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class PostService(IApplicationDbContext db, IMapper mapper) : IPostService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var post = await db.GetAsync<Entities.Post>(id);
        if (post is not null)
        {
            await db.DeleteAsync(post);
            await db.SaveAsync();

            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No post found");
    }

    public async Task<Result<Post>> GetAsync(Guid id)
    {
        var dbPost = await db.GetAsync<Entities.Post>(id);
        if (dbPost is not null)
        {
            var post = mapper.Map<Post>(dbPost);
            return post;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No post found");
    }

    public async Task<Result<IEnumerable<Post>>> GetListAsync()
    {
        var posts = await db.GetData<Entities.Post>()
            .ProjectTo<Post>(mapper.ConfigurationProvider)
            .ToListAsync();

        return posts;
    }

    public async Task<Result<Post>> InsertAsync(SavePostRequest request)
    {
        var dbPost = mapper.Map<Entities.Post>(request);
        await db.InsertAsync(dbPost);

        return mapper.Map<Post>(dbPost);
    }

    public async Task<Result<Post>> UpdateAsync(Guid id, SavePostRequest request)
    {
        var dbPost = await db.GetData<Entities.Post>(trackingChanges: true).FirstOrDefaultAsync(p => p.Id == id);
        if (dbPost is not null)
        {
            mapper.Map(request, dbPost);
            await db.SaveAsync();

            return mapper.Map<Post>(dbPost);
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No post found");
    }
}