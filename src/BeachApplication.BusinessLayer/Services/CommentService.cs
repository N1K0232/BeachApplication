using AutoMapper;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class CommentService(IApplicationDbContext db, IUserService userService, IMapper mapper) : ICommentService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var dbComment = await db.GetAsync<Entities.Comment>(id);
        if (dbComment is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No comment found with id {id}");
        }

        await db.DeleteAsync(dbComment);
        await db.SaveAsync();

        return Result.Ok();
    }

    public async Task<Result<Comment>> GetAsync()
    {
        var userId = await userService.GetIdAsync();
        var dbComment = await db.GetData<Entities.Comment>().FirstOrDefaultAsync(c => c.UserId == userId);

        if (dbComment is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No comment found with id {id}");
        }

        var comment = mapper.Map<Comment>(dbComment);
        return comment;
    }

    public async Task<Result<Comment>> GetAsync(Guid id)
    {
        var dbComment = await db.GetAsync<Entities.Comment>(id);
        if (dbComment is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No comment found with id {id}");
        }

        var comment = mapper.Map<Comment>(dbComment);
        return comment;
    }

    public async Task<Result<PaginatedList<Comment>>> GetListAsync(int pageIndex, int itemsPerPage)
    {
        var query = db.GetData<Entities.Comment>();
        var totalCount = await query.CountAsync();

        var dbComments = await query.OrderByDescending(c => c.CreatedAt).ToListAsync(pageIndex, itemsPerPage);
        var hasNextPage = await query.HasNextPageAsync(pageIndex, itemsPerPage);

        var comments = mapper.Map<IEnumerable<Comment>>(dbComments).Take(itemsPerPage);
        return new PaginatedList<Comment>(comments, totalCount, hasNextPage);
    }

    public async Task<Result<Comment>> InsertAsync(SaveCommentRequest request)
    {
        var dbComment = mapper.Map<Entities.Comment>(request);
        dbComment.UserId = await userService.GetIdAsync();

        await db.InsertAsync(dbComment);
        await db.SaveAsync();

        return mapper.Map<Comment>(dbComment);
    }

    public async Task<Result<Comment>> UpdateAsync(Guid id, SaveCommentRequest request)
    {
        var userId = await userService.GetIdAsync();
        var dbComment = await db.GetData<Entities.Comment>(trackingChanges: true).FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (dbComment is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No comment found with id {id}");
        }

        mapper.Map(request, dbComment);
        await db.SaveAsync();

        return mapper.Map<Comment>(dbComment);
    }
}