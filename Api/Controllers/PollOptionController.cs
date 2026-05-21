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
[Route("api/polloptions")]
public class PollOptionController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public PollOptionController(IUnitOfWork uow, ISender sender, IMapper mapper)
    {
        _uow = uow;
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PollOptionDto>>> GetAllPollOptions()
    {
        var result = await _sender.Send(new GetPollOptionsQuery());
        var response = _mapper.Map<IEnumerable<PollOptionDto>>(result);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<PollOptionDto>> CreatePollOption([FromBody] CreatePollOptionRequest request)
    {
        var command = _mapper.Map<CreatePollOptionCommand>(request);
        var result = await _sender.Send(command);
        var response = _mapper.Map<PollOptionDto>(result);
        return Ok(response);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<IEnumerable<PollOptionDto>>> GetPagedPollOptions(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null)
    {
        var result = await _sender.Send(new GetPagedPollOptionsQuery(page, pageSize, search));
        var response = _mapper.Map<IEnumerable<PollOptionDto>>(result);
        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetPollOptionsCount([FromQuery] string? search = null)
    {
        var count = await _sender.Send(new GetPollOptionsCountQuery(search));
        return Ok(count);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PollOptionDto>> GetPollOptionById(Guid id)
    {
        var result = await _sender.Send(new GetPollOptionByIdQuery(id));
        if (result == null)
            return NotFound(new { Message = $"No se encontró la opción de votación con ID: {id}" });
        var response = _mapper.Map<PollOptionDto>(result);
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PollOptionDto>> UpdatePollOption(Guid id, [FromBody] UpdatePollOptionRequest request)
    {
        var command = new UpdatePollOptionCommand(id, request.Text);
        var result = await _sender.Send(command);
        if (result == null)
            return NotFound(new { Message = $"No se encontró la opción de votación con ID: {id}" });
        var response = _mapper.Map<PollOptionDto>(result);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePollOption(Guid id)
    {
        var success = await _sender.Send(new DeletePollOptionCommand(id));
        if (!success)
            return NotFound(new { Message = $"No se encontró la opción de votación con ID: {id}" });
        return Ok(new { Message = "Opción de votación eliminada con éxito." });
    }
}
