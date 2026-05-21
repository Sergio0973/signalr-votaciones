using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Abstractions;

public interface IPollHubService
{
    Task BroadcastPollUpdatedAsync(Poll poll);
    Task BroadcastPollClosedAsync();
}
