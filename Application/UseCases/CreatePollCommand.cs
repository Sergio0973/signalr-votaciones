using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Domain.Entities;
using Domain.ValueObjects.Polls;
using MediatR;

namespace Application.UseCases;

public record CreatePollCommand(string Question, IReadOnlyList<string> Options) : IRequest<Poll>;

public class CreatePollCommandHandler : IRequestHandler<CreatePollCommand, Poll>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPollHubService _pollHubService;

    public CreatePollCommandHandler(IUnitOfWork unitOfWork, IPollHubService pollHubService)
    {
        _unitOfWork = unitOfWork;
        _pollHubService = pollHubService;
    }

    public async Task<Poll> Handle(CreatePollCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.Polls.ClearActivePollAsync(cancellationToken);

        var question = QuestionText.Create(request.Question);
        var options = request.Options.Select(OptionText.Create).ToList();

        var poll = new Poll(PollId.New(), question, options);
        await _unitOfWork.Polls.SavePollAsync(poll, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _pollHubService.BroadcastPollUpdatedAsync(poll);

        return poll;
    }
}
