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

public class EfPollRepository : IPollRepository
{
    private readonly AppDbContext _context;

    public EfPollRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Poll?> GetActivePollAsync(CancellationToken ct = default)
    {
        return await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .FirstOrDefaultAsync(p => p.IsActive, ct);
    }

    public async Task SavePollAsync(Poll poll, CancellationToken ct = default)
    {
        var exists = await _context.Polls.AnyAsync(p => p.Id == poll.Id, ct);
        if (exists)
            _context.Polls.Update(poll);
        else
            await _context.Polls.AddAsync(poll, ct);
    }

    public async Task ClearActivePollAsync(CancellationToken ct = default)
    {
        var active = await _context.Polls
            .FirstOrDefaultAsync(p => p.IsActive, ct);

        if (active != null)
        {
            active.Close();
            _context.Polls.Update(active);
        }
    }

    public Task<Poll?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Set<Poll>()
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .AsTracking()
            .FirstOrDefaultAsync(p => p.Id == PollId.Create(id), ct);

    public Task<IReadOnlyList<Poll>> GetAllAsync(CancellationToken ct = default) =>
        _context.Set<Poll>()
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Poll>)t.Result, ct);

    public async Task<IReadOnlyList<Poll>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        IQueryable<Poll> query;
        if (string.IsNullOrWhiteSpace(search))
        {
            query = _context.Polls
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .AsNoTracking();
        }
        else
        {
            var pattern = $"%{search.Trim().ToUpper()}%";
            query = _context.Polls
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM polls
                    WHERE UPPER(question) LIKE {pattern}")
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .AsNoTracking();
        }

        return await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? search = null, CancellationToken ct = default)
    {
        IQueryable<Poll> query;
        if (string.IsNullOrWhiteSpace(search))
        {
            query = _context.Polls.AsNoTracking();
        }
        else
        {
            var pattern = $"%{search.Trim().ToUpper()}%";
            query = _context.Polls
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM polls
                    WHERE UPPER(question) LIKE {pattern}")
                .AsNoTracking();
        }
        return query.CountAsync(ct);
    }

    public Task AddAsync(Poll poll, CancellationToken ct = default)
    {
        _context.Polls.Add(poll);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Poll poll, CancellationToken ct = default)
    {
        _context.Polls.Update(poll);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Poll poll, CancellationToken ct = default)
    {
        _context.Set<Poll>().Remove(poll);
        return Task.CompletedTask;
    }
}
