using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Domain.Repositories;

namespace Infrastructure.UnitOfWork;

public class InMemoryUnitOfWork : IUnitOfWork
{
    public IPollRepository Polls { get; }
    public IPollOptionRepository PollOptions => throw new NotImplementedException();
    public IVoteRepository Votes => throw new NotImplementedException();

    public InMemoryUnitOfWork(IPollRepository pollRepository)
    {
        Polls = pollRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return Task.FromResult(1);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        await operation(ct);
    }
}
