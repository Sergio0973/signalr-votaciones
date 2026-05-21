using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.UseCases;

public record ClosePollCommand : IRequest<bool>;

public class ClosePollCommandHandler : IRequestHandler<ClosePollCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPollHubService _pollHubService;

    public ClosePollCommandHandler(IUnitOfWork unitOfWork, IPollHubService pollHubService)
    {
        _unitOfWork = unitOfWork;
        _pollHubService = pollHubService;
    }

    public async Task<bool> Handle(ClosePollCommand request, CancellationToken cancellationToken)
    {
        var poll = await _unitOfWork.Polls.GetActivePollAsync(cancellationToken);
        if (poll == null)
            return false;

        poll.Close();
        await _unitOfWork.Polls.SavePollAsync(poll, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _pollHubService.BroadcastPollUpdatedAsync(poll);
        await _pollHubService.BroadcastPollClosedAsync();

        return true;
    }
}
