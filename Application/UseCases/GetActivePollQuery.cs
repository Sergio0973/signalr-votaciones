using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.UseCases;

public record GetActivePollQuery : IRequest<Poll?>;

public class GetActivePollQueryHandler : IRequestHandler<GetActivePollQuery, Poll?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetActivePollQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Poll?> Handle(GetActivePollQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Polls.GetActivePollAsync(cancellationToken);
    }
}
