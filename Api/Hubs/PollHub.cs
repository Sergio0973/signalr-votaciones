using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Dtos.Polls;
using Application.UseCases;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class PollHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public PollHub(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task CastVote(Guid optionId, string voterId)
    {
        var command = new CastVoteCommand(voterId, optionId);
        bool success = await _mediator.Send(command);

        if (!success)
        {
            await Clients.Caller.SendAsync("VoteFailed", "No se pudo registrar tu voto. Asegúrate de que la votación esté abierta.");
        }
    }

    public async Task CreatePoll(string question, IReadOnlyList<string> options)
    {
        var command = new CreatePollCommand(question, options);
        await _mediator.Send(command);
    }

    public async Task ClosePoll()
    {
        await _mediator.Send(new ClosePollCommand());
    }

    public override async Task OnConnectedAsync()
    {
        var activePoll = await _mediator.Send(new GetActivePollQuery());
        if (activePoll != null)
        {
            var pollDto = _mapper.Map<PollDto>(activePoll);
            await Clients.Caller.SendAsync("PollUpdated", pollDto);
        }
        await base.OnConnectedAsync();
    }
}
