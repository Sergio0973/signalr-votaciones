using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories;

public interface IVoteRepository
{
    Task<Vote?> GetByIdAsync(string voterId, Guid optionId, CancellationToken ct = default);
    Task<IReadOnlyList<Vote>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Vote>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default);
    Task<int> CountAsync(string? search = null, CancellationToken ct = default);
    Task AddAsync(Vote vote, CancellationToken ct = default);
    Task UpdateAsync(Vote vote, CancellationToken ct = default);
    Task RemoveAsync(Vote vote, CancellationToken ct = default);
}
