using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;

namespace Application.Abstractions;

public interface IUnitOfWork
{
    IPollRepository Polls { get; }
    IPollOptionRepository PollOptions { get; }
    IVoteRepository Votes { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default);
}
