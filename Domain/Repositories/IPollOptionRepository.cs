using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories;

public interface IPollOptionRepository
{
    Task<PollOption?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PollOption>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PollOption>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default);
    Task<int> CountAsync(string? search = null, CancellationToken ct = default);
    Task AddAsync(PollOption option, CancellationToken ct = default);
    Task UpdateAsync(PollOption option, CancellationToken ct = default);
    Task RemoveAsync(PollOption option, CancellationToken ct = default);
}
