using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects.Polls;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EfVoteRepository : IVoteRepository
{
    private readonly AppDbContext _context;

    public EfVoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Vote?> GetByIdAsync(string voterId, Guid optionId, CancellationToken ct = default) =>
        _context.Set<Vote>()
            .AsTracking()
            .FirstOrDefaultAsync(v => v.VoterId == VoterId.Create(voterId) && v.OptionId == PollOptionId.Create(optionId), ct);

    public Task<IReadOnlyList<Vote>> GetAllAsync(CancellationToken ct = default) =>
        _context.Set<Vote>().ToListAsync(ct).ContinueWith(t => (IReadOnlyList<Vote>)t.Result, ct);

    public async Task<IReadOnlyList<Vote>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        IQueryable<Vote> query;
        if (string.IsNullOrWhiteSpace(search))
        {
            query = _context.Votes.AsNoTracking();
        }
        else
        {
            var pattern = $"%{search.Trim().ToUpper()}%";
            query = _context.Votes
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM votes
                    WHERE UPPER(voter_id) LIKE {pattern}")
                .AsNoTracking();
        }

        return await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? search = null, CancellationToken ct = default)
    {
        IQueryable<Vote> query;
        if (string.IsNullOrWhiteSpace(search))
        {
            query = _context.Votes.AsNoTracking();
        }
        else
        {
            var pattern = $"%{search.Trim().ToUpper()}%";
            query = _context.Votes
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM votes
                    WHERE UPPER(voter_id) LIKE {pattern}")
                .AsNoTracking();
        }
        return query.CountAsync(ct);
    }

    public Task AddAsync(Vote vote, CancellationToken ct = default)
    {
        _context.Votes.Add(vote);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Vote vote, CancellationToken ct = default)
    {
        _context.Votes.Update(vote);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Vote vote, CancellationToken ct = default)
    {
        _context.Set<Vote>().Remove(vote);
        return Task.CompletedTask;
    }
}
