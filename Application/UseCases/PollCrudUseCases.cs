using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Domain.Entities;
using Domain.ValueObjects.Polls;
using MediatR;

namespace Application.UseCases;

public record GetPollsQuery : IRequest<IReadOnlyList<Poll>>;
public record GetPagedPollsQuery(int Page, int PageSize, string? Search = null) : IRequest<IReadOnlyList<Poll>>;
public record GetPollsCountQuery(string? Search = null) : IRequest<int>;
public record GetPollByIdQuery(Guid Id) : IRequest<Poll?>;
public record UpdatePollCommand(Guid Id, string Question, bool IsActive) : IRequest<Poll?>;
public record DeletePollCommand(Guid Id) : IRequest<bool>;

public class PollCrudUseCasesHandlers :
    IRequestHandler<GetPollsQuery, IReadOnlyList<Poll>>,
    IRequestHandler<GetPagedPollsQuery, IReadOnlyList<Poll>>,
    IRequestHandler<GetPollsCountQuery, int>,
    IRequestHandler<GetPollByIdQuery, Poll?>,
    IRequestHandler<UpdatePollCommand, Poll?>,
    IRequestHandler<DeletePollCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public PollCrudUseCasesHandlers(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Poll>> Handle(GetPollsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Polls.GetAllAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Poll>> Handle(GetPagedPollsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Polls.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
    }

    public async Task<int> Handle(GetPollsCountQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Polls.CountAsync(request.Search, cancellationToken);
    }

    public async Task<Poll?> Handle(GetPollByIdQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Polls.GetByIdAsync(request.Id, cancellationToken);
    }

    public async Task<Poll?> Handle(UpdatePollCommand request, CancellationToken cancellationToken)
    {
        var poll = await _unitOfWork.Polls.GetByIdAsync(request.Id, cancellationToken);
        if (poll == null) return null;

        poll.Update(QuestionText.Create(request.Question), request.IsActive);
        await _unitOfWork.Polls.UpdateAsync(poll, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return poll;
    }

    public async Task<bool> Handle(DeletePollCommand request, CancellationToken cancellationToken)
    {
        var poll = await _unitOfWork.Polls.GetByIdAsync(request.Id, cancellationToken);
        if (poll == null) return false;

        await _unitOfWork.Polls.RemoveAsync(poll, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
