using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Domain.Entities;
using Domain.ValueObjects.Polls;
using MediatR;

namespace Application.UseCases;

public record GetPollOptionsQuery : IRequest<IReadOnlyList<PollOption>>;
public record GetPagedPollOptionsQuery(int Page, int PageSize, string? Search = null) : IRequest<IReadOnlyList<PollOption>>;
public record GetPollOptionsCountQuery(string? Search = null) : IRequest<int>;
public record GetPollOptionByIdQuery(Guid Id) : IRequest<PollOption?>;
public record CreatePollOptionCommand(string Text) : IRequest<PollOption>;
public record UpdatePollOptionCommand(Guid Id, string Text) : IRequest<PollOption?>;
public record DeletePollOptionCommand(Guid Id) : IRequest<bool>;

public class PollOptionCrudUseCasesHandlers :
    IRequestHandler<GetPollOptionsQuery, IReadOnlyList<PollOption>>,
    IRequestHandler<GetPagedPollOptionsQuery, IReadOnlyList<PollOption>>,
    IRequestHandler<GetPollOptionsCountQuery, int>,
    IRequestHandler<GetPollOptionByIdQuery, PollOption?>,
    IRequestHandler<CreatePollOptionCommand, PollOption>,
    IRequestHandler<UpdatePollOptionCommand, PollOption?>,
    IRequestHandler<DeletePollOptionCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public PollOptionCrudUseCasesHandlers(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PollOption>> Handle(GetPollOptionsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.PollOptions.GetAllAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PollOption>> Handle(GetPagedPollOptionsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.PollOptions.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
    }

    public async Task<int> Handle(GetPollOptionsCountQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.PollOptions.CountAsync(request.Search, cancellationToken);
    }

    public async Task<PollOption?> Handle(GetPollOptionByIdQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.PollOptions.GetByIdAsync(request.Id, cancellationToken);
    }

    public async Task<PollOption> Handle(CreatePollOptionCommand request, CancellationToken cancellationToken)
    {
        var option = new PollOption(PollOptionId.New(), OptionText.Create(request.Text));
        await _unitOfWork.PollOptions.AddAsync(option, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return option;
    }

    public async Task<PollOption?> Handle(UpdatePollOptionCommand request, CancellationToken cancellationToken)
    {
        var option = await _unitOfWork.PollOptions.GetByIdAsync(request.Id, cancellationToken);
        if (option == null) return null;

        option.Update(OptionText.Create(request.Text));
        await _unitOfWork.PollOptions.UpdateAsync(option, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return option;
    }

    public async Task<bool> Handle(DeletePollOptionCommand request, CancellationToken cancellationToken)
    {
        var option = await _unitOfWork.PollOptions.GetByIdAsync(request.Id, cancellationToken);
        if (option == null) return false;

        await _unitOfWork.PollOptions.RemoveAsync(option, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
