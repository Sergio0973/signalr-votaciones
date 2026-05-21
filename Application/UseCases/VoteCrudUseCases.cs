using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Domain.Entities;
using Domain.ValueObjects.Polls;
using MediatR;

namespace Application.UseCases;

public record GetVotesQuery : IRequest<IReadOnlyList<Vote>>;
public record GetPagedVotesQuery(int Page, int PageSize, string? Search = null) : IRequest<IReadOnlyList<Vote>>;
public record GetVotesCountQuery(string? Search = null) : IRequest<int>;
public record GetVoteByIdQuery(string VoterId, Guid OptionId) : IRequest<Vote?>;
public record CreateVoteCommand(string VoterId, Guid OptionId) : IRequest<Vote>;
public record UpdateVoteCommand(string VoterId, Guid OptionId) : IRequest<Vote?>;
public record DeleteVoteCommand(string VoterId, Guid OptionId) : IRequest<bool>;

public class VoteCrudUseCasesHandlers :
    IRequestHandler<GetVotesQuery, IReadOnlyList<Vote>>,
    IRequestHandler<GetPagedVotesQuery, IReadOnlyList<Vote>>,
    IRequestHandler<GetVotesCountQuery, int>,
    IRequestHandler<GetVoteByIdQuery, Vote?>,
    IRequestHandler<CreateVoteCommand, Vote>,
    IRequestHandler<UpdateVoteCommand, Vote?>,
    IRequestHandler<DeleteVoteCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public VoteCrudUseCasesHandlers(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Vote>> Handle(GetVotesQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Votes.GetAllAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vote>> Handle(GetPagedVotesQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Votes.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
    }

    public async Task<int> Handle(GetVotesCountQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Votes.CountAsync(request.Search, cancellationToken);
    }

    public async Task<Vote?> Handle(GetVoteByIdQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Votes.GetByIdAsync(request.VoterId, request.OptionId, cancellationToken);
    }

    public async Task<Vote> Handle(CreateVoteCommand request, CancellationToken cancellationToken)
    {
        var vote = new Vote(VoterId.Create(request.VoterId), PollOptionId.Create(request.OptionId));
        await _unitOfWork.Votes.AddAsync(vote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return vote;
    }

    public async Task<Vote?> Handle(UpdateVoteCommand request, CancellationToken cancellationToken)
    {
        var vote = await _unitOfWork.Votes.GetByIdAsync(request.VoterId, request.OptionId, cancellationToken);
        if (vote == null) return null;

        await _unitOfWork.Votes.UpdateAsync(vote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return vote;
    }

    public async Task<bool> Handle(DeleteVoteCommand request, CancellationToken cancellationToken)
    {
        var vote = await _unitOfWork.Votes.GetByIdAsync(request.VoterId, request.OptionId, cancellationToken);
        if (vote == null) return false;

        await _unitOfWork.Votes.RemoveAsync(vote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
