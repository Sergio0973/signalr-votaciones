using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects.Polls;

namespace Infrastructure.Repositories;

public class InMemoryPollRepository : IPollRepository
{
    private static Poll? _activePoll;
    private static readonly object _lock = new();

    public Task<Poll?> GetActivePollAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_activePoll);
        }
    }

    public Task SavePollAsync(Poll poll, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _activePoll = poll;
            return Task.CompletedTask;
        }
    }

    public Task ClearActivePollAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            _activePoll = null;
            return Task.CompletedTask;
        }
    }

    public Task<Poll?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_activePoll?.Id == PollId.Create(id)) return Task.FromResult<Poll?>(_activePoll);
            return Task.FromResult<Poll?>(null);
        }
    }

    public Task<IReadOnlyList<Poll>> GetAllAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            var list = new List<Poll>();
            if (_activePoll != null) list.Add(_activePoll);
            return Task.FromResult<IReadOnlyList<Poll>>(list);
        }
    }

    public Task<IReadOnlyList<Poll>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var list = new List<Poll>();
            if (_activePoll != null)
            {
                if (string.IsNullOrWhiteSpace(search) || _activePoll.Question.Value.ToUpper().Contains(search.Trim().ToUpper()))
                {
                    list.Add(_activePoll);
                }
            }
            return Task.FromResult<IReadOnlyList<Poll>>(list);
        }
    }

    public Task<int> CountAsync(string? search = null, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_activePoll != null)
            {
                if (string.IsNullOrWhiteSpace(search) || _activePoll.Question.Value.ToUpper().Contains(search.Trim().ToUpper()))
                {
                    return Task.FromResult(1);
                }
            }
            return Task.FromResult(0);
        }
    }

    public Task AddAsync(Poll poll, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _activePoll = poll;
            return Task.CompletedTask;
        }
    }

    public Task UpdateAsync(Poll poll, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _activePoll = poll;
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(Poll poll, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_activePoll?.Id == poll.Id) _activePoll = null;
            return Task.CompletedTask;
        }
    }
}
