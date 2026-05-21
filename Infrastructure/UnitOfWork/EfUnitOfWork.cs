using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Domain.Repositories;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UnitOfWork;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IPollRepository? _polls;
    private IPollOptionRepository? _pollOptions;
    private IVoteRepository? _votes;

    public EfUnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IPollRepository Polls => _polls ??= new EfPollRepository(_context);
    public IPollOptionRepository PollOptions => _pollOptions ??= new EfPollOptionRepository(_context);
    public IVoteRepository Votes => _votes ??= new EfVoteRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await operation(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
