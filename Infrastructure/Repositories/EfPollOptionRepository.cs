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

public class EfPollOptionRepository : IPollOptionRepository
{
    private readonly AppDbContext _context;

    public EfPollOptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<PollOption?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Set<PollOption>().AsTracking().FirstOrDefaultAsync(o => o.Id == PollOptionId.Create(id), ct);

    public Task<IReadOnlyList<PollOption>> GetAllAsync(CancellationToken ct = default) =>
        _context.Set<PollOption>().ToListAsync(ct).ContinueWith(t => (IReadOnlyList<PollOption>)t.Result, ct);

    public async Task<IReadOnlyList<PollOption>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        IQueryable<PollOption> query;
        if (string.IsNullOrWhiteSpace(search))
        {
            query = _context.PollOptions.AsNoTracking();
        }
        else
        {
            var pattern = $"%{search.Trim().ToUpper()}%";
            query = _context.PollOptions
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM poll_options
                    WHERE UPPER(text) LIKE {pattern}")
                .AsNoTracking();
        }

        return await query
            .OrderBy(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? search = null, CancellationToken ct = default)
    {
        IQueryable<PollOption> query;
        if (string.IsNullOrWhiteSpace(search))
        {
            query = _context.PollOptions.AsNoTracking();
        }
        else
        {
            var pattern = $"%{search.Trim().ToUpper()}%";
            query = _context.PollOptions
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM poll_options
                    WHERE UPPER(text) LIKE {pattern}")
                .AsNoTracking();
        }
        return query.CountAsync(ct);
    }

    public Task AddAsync(PollOption option, CancellationToken ct = default)
    {
        _context.PollOptions.Add(option);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(PollOption option, CancellationToken ct = default)
    {
        _context.PollOptions.Update(option);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(PollOption option, CancellationToken ct = default)
    {
        _context.Set<PollOption>().Remove(option);
        return Task.CompletedTask;
    }
}
