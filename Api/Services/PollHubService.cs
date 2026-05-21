using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Dtos.Polls;
using Api.Hubs;
using Application.Abstractions;
using Domain.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.SignalR;

namespace Api.Services;

public class PollHubService : IPollHubService
{
    private readonly IHubContext<PollHub> _hubContext;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public PollHubService(IHubContext<PollHub> hubContext, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _hubContext = hubContext;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task BroadcastPollUpdatedAsync(Poll poll)
    {
        var pollDto = _mapper.Map<PollDto>(poll);
        await _hubContext.Clients.All.SendAsync("PollUpdated", pollDto);
    }

    public async Task BroadcastPollClosedAsync()
    {
        await _hubContext.Clients.All.SendAsync("PollClosed");
    }
}
