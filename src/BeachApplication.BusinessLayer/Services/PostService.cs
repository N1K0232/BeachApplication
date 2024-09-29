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
        var post = mapper.Map<Entities.Post>(request);
        await db.InsertAsync(post);

        return mapper.Map<Post>(post);
    }

    public async Task<Result<Post>> UpdateAsync(Guid id, SavePostRequest request)
    {
        var post = await db.GetData<Entities.Post>(trackingChanges: true).FirstOrDefaultAsync(p => p.Id == id);
        if (post is not null)
        {
            mapper.Map(request, post);
            await db.UpdateAsync(post);

            return mapper.Map<Post>(post);
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No post found");
    }
}