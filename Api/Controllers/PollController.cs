using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Dtos.Polls;
using Application.Abstractions;
using Application.UseCases;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/polls")]
public class PollController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public PollController(IUnitOfWork uow, ISender sender, IMapper mapper)
    {
        _uow = uow;
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PollDto>>> GetAllPolls()
    {
        var result = await _sender.Send(new GetPollsQuery());
        var response = _mapper.Map<IEnumerable<PollDto>>(result);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<PollDto>> CreatePoll([FromBody] CreatePollRequest request)
    {
        var command = _mapper.Map<CreatePollCommand>(request);
        var result = await _sender.Send(command);
        var response = _mapper.Map<PollDto>(result);
        return Ok(response);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<IEnumerable<PollDto>>> GetPagedPolls(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null)
    {
        var result = await _sender.Send(new GetPagedPollsQuery(page, pageSize, search));
        var response = _mapper.Map<IEnumerable<PollDto>>(result);
        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetPollsCount([FromQuery] string? search = null)
    {
        var count = await _sender.Send(new GetPollsCountQuery(search));
        return Ok(count);
    }

    [HttpGet("active")]
    public async Task<ActionResult<PollDto>> GetActivePoll()
    {
        var result = await _sender.Send(new GetActivePollQuery());
        if (result == null)
            return NotFound(new { Message = "No hay ninguna votación activa actualmente." });
        var response = _mapper.Map<PollDto>(result);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PollDto>> GetPollById(Guid id)
    {
        var result = await _sender.Send(new GetPollByIdQuery(id));
        if (result == null)
            return NotFound(new { Message = $"No se encontró la votación con ID: {id}" });
        var response = _mapper.Map<PollDto>(result);
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PollDto>> UpdatePoll(Guid id, [FromBody] UpdatePollRequest request)
    {
        var result = await _sender.Send(new UpdatePollCommand(id, request.Question, request.IsActive));
        if (result == null)
            return NotFound(new { Message = $"No se encontró la votación con ID: {id}" });
        var response = _mapper.Map<PollDto>(result);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePoll(Guid id)
    {
        var success = await _sender.Send(new DeletePollCommand(id));
        if (!success)
            return NotFound(new { Message = $"No se encontró la votación con ID: {id}" });
        return Ok(new { Message = "Votación eliminada con éxito." });
    }

    [HttpPost("close")]
    public async Task<IActionResult> ClosePoll()
    {
        var success = await _sender.Send(new ClosePollCommand());
        if (!success)
            return BadRequest(new { Message = "No hay ninguna votación activa para cerrar." });
        return Ok(new { Message = "Votación cerrada con éxito." });
    }
}
