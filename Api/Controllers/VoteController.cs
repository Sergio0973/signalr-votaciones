using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Dtos.Polls;
using Application.Abstractions;
using Application.UseCases;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/votes")]
public class VoteController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public VoteController(IUnitOfWork uow, ISender sender, IMapper mapper)
    {
        _uow = uow;
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VoteDto>>> GetAllVotes()
    {
        var result = await _sender.Send(new GetVotesQuery());
        var response = _mapper.Map<IEnumerable<VoteDto>>(result);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<VoteDto>> CreateVote([FromBody] CreateVoteRequest request)
    {
        var command = _mapper.Map<CreateVoteCommand>(request);
        var result = await _sender.Send(command);
        var response = _mapper.Map<VoteDto>(result);
        return Ok(response);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<IEnumerable<VoteDto>>> GetPagedVotes(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null)
    {
        var result = await _sender.Send(new GetPagedVotesQuery(page, pageSize, search));
        var response = _mapper.Map<IEnumerable<VoteDto>>(result);
        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetVotesCount([FromQuery] string? search = null)
    {
        var count = await _sender.Send(new GetVotesCountQuery(search));
        return Ok(count);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VoteDto>> GetVoteById(string id)
    {
        var (voterId, optionId) = ParseCompositeId(id);
        var result = await _sender.Send(new GetVoteByIdQuery(voterId, optionId));
        if (result == null)
            return NotFound(new { Message = $"No se encontró el voto con ID: {id} (Formato esperado: voterId_optionId)" });
        var response = _mapper.Map<VoteDto>(result);
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VoteDto>> UpdateVote(string id)
    {
        var (voterId, optionId) = ParseCompositeId(id);
        var result = await _sender.Send(new UpdateVoteCommand(voterId, optionId));
        if (result == null)
            return NotFound(new { Message = $"No se encontró el voto con ID: {id} (Formato esperado: voterId_optionId)" });
        var response = _mapper.Map<VoteDto>(result);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVote(string id)
    {
        var (voterId, optionId) = ParseCompositeId(id);
        var success = await _sender.Send(new DeleteVoteCommand(voterId, optionId));
        if (!success)
            return NotFound(new { Message = $"No se encontró el voto con ID: {id} (Formato esperado: voterId_optionId)" });
        return Ok(new { Message = "Voto eliminado con éxito." });
    }

    private static (string VoterId, Guid OptionId) ParseCompositeId(string id)
    {
        if (string.IsNullOrEmpty(id)) return (string.Empty, Guid.Empty);
        var parts = id.Split('_');
        if (parts.Length == 2 && Guid.TryParse(parts[1], out var parsedGuid))
        {
            return (parts[0], parsedGuid);
        }
        return (id, Guid.Empty);
    }
}
