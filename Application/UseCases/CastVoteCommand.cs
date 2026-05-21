using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Domain.ValueObjects.Polls;
using MediatR;

namespace Application.UseCases;

public record CastVoteCommand(string VoterId, Guid OptionId) : IRequest<bool>;

public class CastVoteCommandHandler : IRequestHandler<CastVoteCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPollHubService _pollHubService;

    public CastVoteCommandHandler(IUnitOfWork unitOfWork, IPollHubService pollHubService)
    {
        _unitOfWork = unitOfWork;
        _pollHubService = pollHubService;
    }

    public async Task<bool> Handle(CastVoteCommand request, CancellationToken cancellationToken)
    {
        var poll = await _unitOfWork.Polls.GetActivePollAsync(cancellationToken);
        if (poll == null)
            return false;

        var voterId = VoterId.Create(request.VoterId);
        var optionId = PollOptionId.Create(request.OptionId);

        bool success = poll.CastVote(voterId, optionId);
        if (!success)
            return false;

        await _unitOfWork.Polls.SavePollAsync(poll, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _pollHubService.BroadcastPollUpdatedAsync(poll);

        return true;
    }
}
