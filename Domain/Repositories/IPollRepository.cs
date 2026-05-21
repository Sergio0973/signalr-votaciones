using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories;

public interface IPollRepository
{
    // Existing active poll methods
    Task<Poll?> GetActivePollAsync(CancellationToken ct = default);
    Task SavePollAsync(Poll poll, CancellationToken ct = default);
    Task ClearActivePollAsync(CancellationToken ct = default);

    // Standard CRUD methods structured like ProductRepository
    Task<Poll?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Poll>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Poll>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default);
    Task<int> CountAsync(string? search = null, CancellationToken ct = default);
    Task AddAsync(Poll poll, CancellationToken ct = default);
    Task UpdateAsync(Poll poll, CancellationToken ct = default);
    Task RemoveAsync(Poll poll, CancellationToken ct = default);
}
